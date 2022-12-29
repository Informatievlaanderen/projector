namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Microsoft.Data.SqlClient;

    public class QueryEventsHandler : HandlerBase, IRequestHandler<QueryEventsRequest, Result<QueryRow>>
    {
        private static readonly Dictionary<string, EventDatabaseInfo> eventDatabaseInfoMap = new Dictionary<string, EventDatabaseInfo>
        {
            // organisation-registry, publicservice-registry, road-registry are not included because they use different database schema's
            // buildingunit-registry is missing an index
            ["address-registry"] = new EventDatabaseInfo("PersistentLocalId", "AddressId", "AddressRegistryLegacy", "AddressSyndication", "PersistentLocalId", "AddressRegistryExtract", "Address", "AddressPersistentLocalId"),
            ["building-registry"] = new EventDatabaseInfo(@"declare @internalId as uniqueidentifier
select top 1 @internalId = BuildingId from BuildingRegistryLegacy.BuildingSyndication where PersistentLocalId is not null and PersistentLocalId = {{ :businessId: }}

select
	convert(varchar(50), d.PersistentLocalId) ExternalId
	,convert(varchar(50), d.BuildingId) InternalId
	,convert(varchar(50), d.Position) EventId
	,d.ChangeType ChangeType
	,d.EventDataAsXml [EventData]
	,convert(varchar(25), d.SyndicationItemCreatedAt, 121) [Timestamp]
from
	BuildingRegistryLegacy.BuildingSyndication d
	left join BuildingRegistryExtract.Building m on m.BuildingId = d.BuildingId
where
	d.BuildingId = @internalId
order by
	d.Position desc
	,d.RecordCreatedAt desc
	,d.SyndicationItemCreatedAt desc
"),
            ["buildingunit-registry"] = new EventDatabaseInfo(@"declare @internalId as uniqueidentifier
select top 1 @internalId = BuildingUnitId from BuildingRegistryLegacy.BuildingUnitSyndication where PersistentLocalId is not null and PersistentLocalId = {{ :businessId: }}

select
	convert(varchar(50), d.PersistentLocalId) ExternalId
	,convert(varchar(50), d.BuildingUnitId) InternalId
	,convert(varchar(50), d.Position) EventId
	--,d.ChangeType ChangeType
	--,d.EventDataAsXml [EventData]
	--,convert(varchar(25), d.SyndicationItemCreatedAt, 121) [Timestamp]
from
	BuildingRegistryLegacy.BuildingUnitSyndication d
	left join BuildingRegistryExtract.BuildingUnit m on m.BuildingUnitId = d.BuildingUnitId
where
	d.BuildingUnitId = @internalId
order by
	d.Position desc
	--,d.RecordCreatedAt desc
	--,d.SyndicationItemCreatedAt desc
"),
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

            var cmdText = !string.IsNullOrEmpty(eventDatabaseInfo.Sql)
                ? eventDatabaseInfo.Sql
                : $@"declare @internalId as varchar(50)
select top 1 @internalId = convert(varchar(50), {eventDatabaseInfo.InternalId}) from {eventDatabaseInfo.DetailSchemaName}.{eventDatabaseInfo.DetailTableName} where {eventDatabaseInfo.ExternalId} is not null and {eventDatabaseInfo.ExternalId} = '{businessId}'

select
	convert(varchar(50), d.{eventDatabaseInfo.ExternalId}) ExternalId
	,convert(varchar(50), d.{eventDatabaseInfo.InternalId}) InternalId
	,convert(varchar(50), d.Position) EventId
	,convert(varchar(25), d.ChangeType) ChangeType
	,d.EventDataAsXml [EventData]
	,convert(varchar(25), d.SyndicationItemCreatedAt, 121) [Timestamp]
from
	{eventDatabaseInfo.DetailSchemaName}.{eventDatabaseInfo.DetailTableName} d
	left join {eventDatabaseInfo.MainSchemaName}.{eventDatabaseInfo.MainTableName} m on m.{eventDatabaseInfo.MainJoinColumnName} = d.{eventDatabaseInfo.DetailJoinColumnName}
where
	convert(varchar(50), d.{eventDatabaseInfo.InternalId}) = @internalId
order by
    d.Position desc
   	,d.RecordCreatedAt desc
	,d.SyndicationItemCreatedAt desc
";

            // variables substitution
            var toReplace = cmdText.StringBetweenMustaches();
            if (toReplace.Equals(":businessId:", StringComparison.InvariantCultureIgnoreCase))
            {
                cmdText = cmdText.Replace(cmdText.StringWithMustaches(), businessId);
            }

            try
            {
                var result = await ExecuteQuery<QueryRow>(connectionString, cmdText);
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

    public record QueryEventsRequest(string ConnectionString, string RegistryName, string ExternalId);
}
