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

        [HttpGet("query/table/{schemaName}/{tableName}")]
        public async Task<IActionResult> Query(
            [FromRoute] string schemaName,
            [FromRoute] string tableName)
        {
            var connectionString = _connectionStringBySchema[schemaName.ToUpperInvariant()];
            var request = new QueryRequest(connectionString, schemaName, tableName, Request.Query.ToDictionary(x => x.Key, x => x.Value.FirstOrDefault()));
            var handler = new QueryHandler();
            var response = await handler.Handle(request).ConfigureAwait(true);

            return response.IsSuccess
                ? Ok(response.Values)
                : BadRequest(response.Error);
        }

        [HttpGet("query/events/{registry}/{externalId}")]
        public async Task<IActionResult> QueryEvents(
            [FromRoute] string registry,
            [FromRoute] string externalId)
        {
            var connectionString = _connectionStringBySchema.Values.FirstOrDefault();
            if (connectionString == null)
            {
                return BadRequest("No connection string is available");
            }

            var request = new QueryEventsRequest(connectionString, registry, externalId);
            var handler = new QueryEventsHandler();
            var response = await handler.Handle(request).ConfigureAwait(true);

            return response.IsSuccess
                ? Ok(response.Values)
                : BadRequest(response.Error);
        }

        protected void RegisterConnectionString(string schema, string connectionString)
            => _connectionStringBySchema.Add(schema.ToUpperInvariant(), connectionString);
    }
}
