namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using System;
    using System.Linq;
    using ConnectedProjections;
    using Microsoft.AspNetCore.Mvc;

    public abstract class DefaultProjectorController : ControllerBase
    {
        protected readonly IConnectedProjectionsManager ProjectionManager;

        protected DefaultProjectorController(IConnectedProjectionsManager connectedProjectionsManager) => ProjectionManager = connectedProjectionsManager;

        [HttpGet]
        public IActionResult Get() => Ok(ProjectionManager.GetRegisteredProjections());

        [HttpPost("start/all")]
        public async Task<IActionResult> Start(CancellationToken cancellationToken)
        {
            await ProjectionManager.Start(cancellationToken);
            return Ok();
        }

        [HttpPost("start/{projectionName}")]
        public async Task<IActionResult> Start(string projectionName, CancellationToken cancellationToken)
        {
            if (!DoesNameExists(projectionName))
                return BadRequest("Invalid projection name.");

            await ProjectionManager.Start(projectionName, cancellationToken);
            
            return Ok();
        }

        [HttpPost("stop/all")]
        public async Task<IActionResult> Stop(CancellationToken cancellationToken)
        {
            await ProjectionManager.Stop(cancellationToken);
            return Ok();
        }

        [HttpPost("stop/{projectionName}")]
        public async Task<IActionResult> Stop(string projectionName, CancellationToken cancellationToken)
        {
            if (!DoesNameExists(projectionName))
                return BadRequest("Invalid projection name.");

            await ProjectionManager.Stop(projectionName, cancellationToken);
            
            return Ok();
        }

        private bool DoesNameExists(string projectionName)
        {
            var projections = ProjectionManager.GetRegisteredProjections();
            return projections.Any(x => string.Equals(x.Name, projectionName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
