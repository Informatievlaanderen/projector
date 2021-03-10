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
            _baseUri = baseUri.TrimEnd('/');
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

        private async Task<IEnumerable<ProjectionResponse>> CreateProjectionResponses(
            IEnumerable<RegisteredConnectedProjection> registeredConnectedProjections,
            CancellationToken cancellationToken)
        {
            var projectionStates = (await _projectionManager.GetProjectionStates(cancellationToken)).ToList();

            return registeredConnectedProjections.Aggregate(
                new List<ProjectionResponse>(),
                (list, projection) =>
                {
                    var projectionState = projectionStates.SingleOrDefault(x => x.Name == projection.Id);
                    list.Add(new ProjectionResponse(
                        projection,
                        projectionState,
                        _baseUri));
                    return list;
                });
        }

        [HttpPost("start/all")]
        public async Task<IActionResult> Start(CancellationToken cancellationToken)
        {
            await _projectionManager.Start(cancellationToken);
            return Accepted();
        }

        [HttpPost("start/{projection}")]
        public async Task<IActionResult> Start(string projection, CancellationToken cancellationToken)
        {
            if (!_projectionManager.Exists(projection))
                return BadRequest("Invalid projection Id.");

            await _projectionManager.Start(projection, cancellationToken);

            return Accepted();
        }

        [HttpPost("stop/all")]
        public async Task<IActionResult> Stop(CancellationToken cancellationToken)
        {
            await _projectionManager.Stop(cancellationToken);
            return Accepted();
        }

        [HttpPost("stop/{projection}")]
        public async Task<IActionResult> Stop(string projection, CancellationToken cancellationToken)
        {
            if (!_projectionManager.Exists(projection))
                return BadRequest("Invalid projection Id.");

            await _projectionManager.Stop(projection, cancellationToken);

            return Accepted();
        }
    }
}
