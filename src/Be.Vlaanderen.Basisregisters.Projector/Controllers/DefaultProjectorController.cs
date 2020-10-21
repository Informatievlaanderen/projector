namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Microsoft.AspNetCore.Mvc;
    using SqlStreamStore;

    public abstract partial class DefaultProjectorController : ControllerBase
    {
        private readonly IConnectedProjectionsManager _projectionManager;
        private readonly string _baseUri;

        protected DefaultProjectorController(
            IConnectedProjectionsManager connectedProjectionsManager,
            string baseUri)
        {
            _projectionManager = connectedProjectionsManager;
            _baseUri = baseUri;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] IStreamStore streamStore,
            CancellationToken cancellationToken)
        {
            var registeredConnectedProjections = _projectionManager
                .GetRegisteredProjections()
                .ToList();

            var responses = await CreateProjectionResponses(registeredConnectedProjections, cancellationToken);
            var streamPosition = await streamStore.ReadHeadPosition(cancellationToken);

            return Ok(new ProjectionResponseList(responses, _baseUri)
            {
                StreamPosition = streamPosition
            });
        }

        private async Task<IEnumerable<ProjectionResponse>> CreateProjectionResponses(IEnumerable<RegisteredConnectedProjection> registeredConnectedProjections, CancellationToken cancellationToken)
        {
            var projectionResponses = new List<ProjectionResponse>();

            var projectionStates = await _projectionManager.GetProjectionStates(cancellationToken);
            foreach (var connectedProjection in registeredConnectedProjections)
            {
                var projectionState = projectionStates.SingleOrDefault(x => x.Name == connectedProjection.Name);
                projectionResponses.Add(new ProjectionResponse(connectedProjection, projectionState, _baseUri));
            }

            return projectionResponses;
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
