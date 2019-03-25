namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using ConnectedProjections;
    using Microsoft.AspNetCore.Mvc;

    public abstract class DefaultProjectorController : ControllerBase
    {
        protected readonly IConnectedProjectionsManager ProjectionManager;

        protected DefaultProjectorController(IConnectedProjectionsManager connectedProjectionsManager) => ProjectionManager = connectedProjectionsManager;

        [HttpGet]
        public IActionResult Get() => Ok(ProjectionManager.GetRegisteredProjections());

        [HttpPost("start/all")]
        public IActionResult Start()
        {
            ProjectionManager.Start();
            return Ok();
        }

        [HttpPost("start/{projectionName}")]
        public IActionResult Start(string projectionName)
        {
            ProjectionManager.Start(projectionName);
            return Ok();
        }

        [HttpPost("stop/all")]
        public IActionResult Stop()
        {
            ProjectionManager.Stop();
            return Ok();
        }

        [HttpPost("stop/{projectionName}")]
        public IActionResult Stop(string projectionName)
        {
            ProjectionManager.Stop(projectionName);
            return Ok();
        }
    }
}
