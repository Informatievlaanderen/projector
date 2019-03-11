namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Linq;
    using ConnectedProjections;
    using Messages;
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
                    });

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
            ProjectionManager.Send<StartAllProjectionsRequested>();
            return Ok();
        }

        [HttpPost("start/{projectionName}")]
        public IActionResult Start(string projectionName)
        {
            var projection = ProjectionManager.FindRegisteredProjectionFor(projectionName);
            ProjectionManager.Send(new StartProjectionRequested(projection));
            return Ok();
        }

        [HttpPost("stop/all")]
        public IActionResult Stop()
        {
            ProjectionManager.Send<StopAllProjectionsRequested>();
            return Ok();
        }

        [HttpPost("stop/{projectionName}")]
        public IActionResult Stop(string projectionName)
        {
            var projection = ProjectionManager.FindRegisteredProjectionFor(projectionName);
            ProjectionManager.Send(new StopProjectionRequested(projection));
            return Ok();
        }
    }
}
