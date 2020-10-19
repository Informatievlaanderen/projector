namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Microsoft.AspNetCore.Mvc;

    public abstract partial class DefaultProjectorController : ControllerBase
    {
        private readonly IConnectedProjectionsManager _projectionManager;

        protected DefaultProjectorController(IConnectedProjectionsManager connectedProjectionsManager)
            => _projectionManager = connectedProjectionsManager;

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var registeredConnectedProjections = _projectionManager
                .GetRegisteredProjections()
                .Select(x => new ProjectionResponse(x));

            await UpdatePositions(cancellationToken, registeredConnectedProjections);

            return Ok(registeredConnectedProjections);
        }

        private async Task UpdatePositions(CancellationToken cancellationToken, IEnumerable<ProjectionResponse> registeredConnectedProjections)
        {
            var positions = await _projectionManager.GetLastSavedPositionsByName(cancellationToken);
            foreach (var position in positions)
            {
                var projection = registeredConnectedProjections.Single(x => x.ProjectionName == position.Key);
                projection.CurrentPosition = position.Value;
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
