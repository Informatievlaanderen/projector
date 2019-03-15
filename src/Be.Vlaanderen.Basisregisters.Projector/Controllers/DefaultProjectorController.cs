namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Threading.Tasks;
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
        public async Task<IActionResult> Start()
        {
            await ProjectionManager.Send<StartAll>();
            return Ok();
        }

        [HttpPost("start/{projectionName}")]
        public async Task<IActionResult> Start(string projectionName)
        {
            var projection = ProjectionManager.GetRegisteredProjectionName(projectionName);
            await ProjectionManager.Send(new Start(projection));
            return Ok();
        }

        [HttpPost("stop/all")]
        public async Task<IActionResult> Stop()
        {
            await ProjectionManager.Send<StopAll>();
            return Ok();
        }

        [HttpPost("stop/{projectionName}")]
        public async Task<IActionResult> Stop(string projectionName)
        {
            var projection = ProjectionManager.GetRegisteredProjectionName(projectionName);
            await ProjectionManager.Send(new Stop(projection));
            return Ok();
        }
    }
}
