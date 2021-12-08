namespace Be.Vlaanderen.Basisregisters.Projector.Handlers
{
    using Controllers;
    using Microsoft.Data.SqlClient;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class QueryEventsHandler : HandlerBase, IRequestHandler<QueryEventsRequest, Result<QueryRow>>
    {
        private static readonly Dictionary<string, EventDatabaseInfo> eventDatabaseInfoMap = new Dictionary<string, EventDatabaseInfo>
        {
            // organisation-registry, publicservice-registry, road-registry are not included because they use different database schema's
            ["address-registry"] = new EventDatabaseInfo("PersistentLocalId", "AddressId", "AddressRegistryLegacy", "AddressSyndication", "PersistentLocalId", "AddressRegistryExtract", "Address", "AddressPersistentLocalId"),
            ["building-registry"] = new EventDatabaseInfo("PersistentLocalId", "BuildingId", "BuildingRegistryLegacy", "BuildingSyndication", "PersistentLocalId", "BuildingRegistryExtract", "Building", "PersistentLocalId"),
            ["municipality-registry"] = new EventDatabaseInfo("NisCode", "MunicipalityId", "MunicipalityRegistryLegacy", "MunicipalitySyndication", "MunicipalityId", "MunicipalityRegistryExtract", "Municipality", "MunicipalityId"),
            ["parcel-registry"] = new EventDatabaseInfo("CaPaKey", "ParcelId", "ParcelRegistryLegacy", "ParcelSyndication", "CaPaKey", "ParcelRegistryExtract", "Parcel", "CaPaKey"),
            ["postal-registry"] = new EventDatabaseInfo("PostalCode", "PostalCode", "PostalRegistryLegacy", "PostalInformationSyndication", "PostalCode", "PostalRegistryExtract", "Postal", "PostalCode"),
            ["streetname-registry"] = new EventDatabaseInfo("PersistentLocalId", "StreetNameId", "StreetNameRegistryLegacy", "StreetNameSyndication", "PersistentLocalId", "StreetNameRegistryExtract", "StreetName", "StreetNamePersistentLocalId")
        };

        public async Task<Result<QueryRow>> Handle(QueryEventsRequest request)
        {
            (string? connectionString, string? registryName, string? businessId) = request;
            if (!eventDatabaseInfoMap.TryGetValue(registryName, out var eventDatabaseInfo))
            {
                return Result<QueryRow>.Failure("Invalid registry was specified");
            }

            var cmdText = $@"select
	convert(varchar(50), d.{eventDatabaseInfo.ExternalId}) ExternalId
	,convert(varchar(50), d.{eventDatabaseInfo.InternalId}) InternalId
	,convert(varchar(50), d.Position) EventId
	,d.ChangeType ChangeType
	,d.EventDataAsXml [EventData]
	,convert(varchar(25), d.SyndicationItemCreatedAt, 121) [Timestamp]
from
	{eventDatabaseInfo.DetailSchemaName}.{eventDatabaseInfo.DetailTableName} d
	left join {eventDatabaseInfo.MainSchemaName}.{eventDatabaseInfo.MainTableName} m on m.{eventDatabaseInfo.MainJoinColumnName} = d.{eventDatabaseInfo.DetailJoinColumnName}
where
	d.{eventDatabaseInfo.InternalId} = '{businessId}'
order by
    d.Position desc
   	,d.RecordCreatedAt desc
	,d.SyndicationItemCreatedAt desc
";

            try
            {
                var result = await ExecuteQuery(connectionString, cmdText);
                return Result<QueryRow>.Success(result);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 207)
                {
                    return Result<QueryRow>.Failure("Invalid query parameter.");
                }

                throw;
            }
        }
    }

    public record QueryEventsRequest(string ConnectionString, string RegistryName, string InternalId);
}
