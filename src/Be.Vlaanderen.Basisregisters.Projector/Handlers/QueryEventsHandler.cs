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
            ["municipality-registry"] = new EventDatabaseInfo("NisCode", "MunicipalityId", "MunicipalityRegistryLegacy", "MunicipalitySyndication", "MunicipalityId", "MunicipalityRegistryExtract", "Municipality", "MunicipalityId"),
            ["address-registry"] = new EventDatabaseInfo("PersistentLocalId", "AddressId", "AddressRegistryLegacy", "AddressSyndication", "PersistentLocalId", "AddressRegistryExtract", "Address", "AddressPersistentLocalId")
        };

        public async Task<Result<QueryRow>> Handle(QueryEventsRequest request)
        {
            (string? connectionString, string? registryName, string? businessId) = request;
            if (!eventDatabaseInfoMap.TryGetValue(registryName, out var eventDatabaseInfo))
            {
                return Result<QueryRow>.Failure("Invalid registry was specified");
            }

            var cmdText = $@"select
    d.{eventDatabaseInfo.ExternalId} ExternalId
    ,d.{eventDatabaseInfo.InternalId} InternalId
	,d.Position EventId
	,d.ChangeType ChangeType
from
	{eventDatabaseInfo.DetailSchemaName}.{eventDatabaseInfo.DetailTableName} d
	left join {eventDatabaseInfo.MainSchemaName}.{eventDatabaseInfo.MainTableName} m on m.{eventDatabaseInfo.MainJoinColumnName} = d.{eventDatabaseInfo.DetailJoinColumnName}
where
	d.{eventDatabaseInfo.InternalId} = '{businessId}'
order by
   	d.RecordCreatedAt desc
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

    public record QueryEventsRequest(string ConnectionString, string RegistryName, string BusinessId);

    public record QueryEventsResponse;
}
