namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using Commands;
    using ConnectedProjections;
    using Microsoft.AspNetCore.Mvc;

    public abstract class DefaultProjectorController : ControllerBase
    {
        protected readonly IConnectedProjectionsManager ProjectionManager;

        protected DefaultProjectorController(IConnectedProjectionsManager connectedProjectionsManager) => ProjectionManager = connectedProjectionsManager;

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(ProjectionManager.GetRegisteredProjections());
        }

        [HttpPost("start/all")]
        public IActionResult Start()
        {
            ProjectionManager.Send<StartAll>();
            return Ok();
        }

        [HttpPost("start/{projectionName}")]
        public IActionResult Start(string projectionName)
        {
            var projection = ProjectionManager.GetRegisteredProjectionName(projectionName);
            ProjectionManager.Send(new Start(projection));
            return Ok();
        }

        [HttpPost("stop/all")]
        public IActionResult Stop()
        {
            ProjectionManager.Send<StopAll>();
            return Ok();
        }

        [HttpPost("stop/{projectionName}")]
        public IActionResult Stop(string projectionName)
        {
            var projection = ProjectionManager.GetRegisteredProjectionName(projectionName);
            ProjectionManager.Send(new Stop(projection));
            return Ok();
        }
    }
}
