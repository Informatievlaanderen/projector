namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Collections.Generic;
    using ConnectedProjections;

    public class ProjectionResponseList
    {
        public IEnumerable<ProjectionResponse> Projections { get; set; }

        public ProjectionResponseList(IEnumerable<ProjectionResponse> projections)
        {
            Projections = projections;
        }
    }

    public class ProjectionResponse
    {
        public ConnectedProjectionName ProjectionName { get; set; }
        public ConnectedProjectionState ProjectionState { get; set; }
        public long CurrentPosition { get; set; }

        public ProjectionResponse(RegisteredConnectedProjection projection)
        {
            ProjectionName = projection.Name;
            ProjectionState = projection.State;
        }
    }
}
