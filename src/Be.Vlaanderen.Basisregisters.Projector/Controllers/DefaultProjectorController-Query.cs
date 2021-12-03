namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Handlers;
    using Microsoft.AspNetCore.Mvc;

    public abstract partial class DefaultProjectorController
    {
        private readonly Dictionary<string, string> _connectionStringBySchema = new Dictionary<string, string>();

        [HttpGet("query/table/{schema}/{table}")]
        public async Task<IActionResult> Query(
            [FromRoute] string schema,
            [FromRoute] string table)
        {
            var connectionString = _connectionStringBySchema[schema.ToUpperInvariant()];
            var request = new QueryRequest(connectionString, schema, table, Request.Query.ToDictionary(x => x.Key, x => x.Value.FirstOrDefault()));
            var handler = new QueryHandler();
            var response = await handler.Handle(request);

            return response.IsSuccess
                ? Ok(response.Values)
                : BadRequest(response.Error);
        }

        [HttpGet("query/events/{registry}/{id}")]
        public async Task<IActionResult> QueryEvents([FromRoute] string registry, [FromRoute] string businessId)
        {
            var connectionString = _connectionStringBySchema.Values.FirstOrDefault();
            if (connectionString == null)
            {
                return BadRequest("No connection string is available");
            }

            var request = new QueryEventsRequest(connectionString, registry, businessId);
            var handler = new QueryEventsHandler();
            var response = await handler.Handle(request);

            return response.IsSuccess
                ? Ok(response.Values)
                : BadRequest(response.Error);
        }

        protected void RegisterConnectionString(string schema, string connectionString)
            => _connectionStringBySchema.Add(schema.ToUpperInvariant(), connectionString);
    }

    internal class EventDatabaseInfo
    {
        public string ExternalId { get; }
        public string InternalId { get; }
        public string DetailSchemaName { get; }
        public string DetailTableName { get; }
        public string DetailJoinColumnName { get; }
        public string MainSchemaName { get; }
        public string MainTableName { get; }
        public string MainJoinColumnName { get; }

        public EventDatabaseInfo(
            string externalId,
            string internalId,
            string detailSchemaName,
            string detailTableName,
            string detailJoinColumnName,
            string mainSchemaName,
            string mainTableName,
            string mainJoinColumnName)
        {
            ExternalId = externalId;
            InternalId = internalId;
            DetailSchemaName = detailSchemaName;
            DetailTableName = detailTableName;
            DetailJoinColumnName = detailJoinColumnName;
            MainSchemaName = mainSchemaName;
            MainTableName = mainTableName;
            MainJoinColumnName = mainJoinColumnName;
        }
    }
}
