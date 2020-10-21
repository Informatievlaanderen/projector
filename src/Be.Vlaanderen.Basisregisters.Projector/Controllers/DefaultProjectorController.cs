namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Dapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;

    public abstract partial class DefaultProjectorController : ControllerBase
    {
        private readonly IConnectedProjectionsManager _projectionManager;
        private readonly string _eventsSchema;
        private readonly string _eventsConnectionString;

        protected DefaultProjectorController(
            IConnectedProjectionsManager connectedProjectionsManager,
            string eventsSchema,
            string eventsConnectionString)
        {
            _projectionManager = connectedProjectionsManager;
            _eventsSchema = eventsSchema;
            _eventsConnectionString = eventsConnectionString;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            CancellationToken cancellationToken)
        {
            var registeredConnectedProjections = _projectionManager
                .GetRegisteredProjections()
                .Select(x => new ProjectionResponse(x))
                .ToList();

            await UpdateWithProjectionState(registeredConnectedProjections, cancellationToken);
            var streamPosition = await GetStreamPosition(cancellationToken);

            var projectionsList = new ProjectionResponseList(registeredConnectedProjections)
            {
                StreamPosition = streamPosition
            };

            return Ok(projectionsList);
        }

        private async Task<long> GetStreamPosition(CancellationToken cancellationToken)
        {
            var streamPosition = -1L;

            using (var connection = new SqlConnection(_eventsConnectionString))
            {
                streamPosition =
                    await connection.ExecuteScalarAsync<long>($"SELECT MAX([Position]) FROM [{_eventsSchema}].[Messages]", cancellationToken);
            }

            return streamPosition;
        }

        private async Task UpdateWithProjectionState(List<ProjectionResponse> registeredConnectedProjections, CancellationToken cancellationToken)
        {
            var projectionStates = await _projectionManager.GetProjectionStates(cancellationToken);
            foreach (var projectionState in projectionStates)
            {
                var projection = registeredConnectedProjections.SingleOrDefault(x => x.ProjectionName == projectionState.Name);
                if (projection != null)
                {
                    projection.CurrentPosition = projection.CurrentPosition;
                    projection.ErrorMessage = projection.ErrorMessage;
                }
            }
        }

        [HttpPost("start/all")]
        public async Task<IActionResult> Start(CancellationToken cancellationToken)
        {
            await _projectionManager.Start(cancellationToken);
            return Accepted();
        }

        [HttpPost("start/{projectionName}")]
        public async Task<IActionResult> Start(string projectionName, CancellationToken cancellationToken)
        {
            if (!_projectionManager.Exists(projectionName))
                return BadRequest("Invalid projection name.");

            await _projectionManager.Start(projectionName, cancellationToken);

            return Accepted();
        }

        [HttpPost("stop/all")]
        public async Task<IActionResult> Stop(CancellationToken cancellationToken)
        {
            await _projectionManager.Stop(cancellationToken);
            return Accepted();
        }

        [HttpPost("stop/{projectionName}")]
        public async Task<IActionResult> Stop(string projectionName, CancellationToken cancellationToken)
        {
            if (!_projectionManager.Exists(projectionName))
                return BadRequest("Invalid projection name.");

            await _projectionManager.Stop(projectionName, cancellationToken);

            return Accepted();
        }
    }
}
