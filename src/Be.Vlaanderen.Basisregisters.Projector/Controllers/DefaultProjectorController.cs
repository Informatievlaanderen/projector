namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Linq;
    using ConnectedProjections;
    using Microsoft.AspNetCore.Mvc;

    public abstract class DefaultProjectorController : ControllerBase
    {
        protected readonly ConnectedProjectionsManager ProjectionManager;

        protected DefaultProjectorController(ConnectedProjectionsManager connectedProjectionsManager) => ProjectionManager = connectedProjectionsManager;

        [HttpGet]
        public IActionResult Get()
        {
            var registeredProjections = ProjectionManager
                .ConnectedProjections
                .Select(
                    projection => new
                    {
                        Name = projection.Name.ToString(),
                        projection.State
                    }
                );

            var response = new
            {
                SubscriptionStream = ProjectionManager.SubscriptionStreamStatus,
                Projections = registeredProjections
            };

            return Ok(response);
        }

        [HttpPost("start/all")]
        public IActionResult Start()
        {
            ProjectionManager.StartAllProjections();
            return Ok();
        }

        [HttpPost("start/{projectionName}")]
        public IActionResult Start(string projectionName)
        {
            ProjectionManager.TryStartProjection(projectionName);
            return Ok();
        }

        [HttpPost("stop/all")]
        public IActionResult Stop()
        {
            ProjectionManager.StopAllProjections();
            return Ok();
        }

        [HttpPost("stop/{projectionName}")]
        public IActionResult Stop(string projectionName)
        {
            ProjectionManager.TryStopProjection(projectionName);
            return Ok();
        }
    }
}
