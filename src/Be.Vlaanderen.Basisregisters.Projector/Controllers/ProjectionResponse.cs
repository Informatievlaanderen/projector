namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Collections.Generic;
    using ConnectedProjections;

    public class ProjectionResponseList
    {
        public IEnumerable<ProjectionResponse> Projections { get; set; }
        public long StreamPosition { get; set; }

        public ProjectionResponseList(IEnumerable<ProjectionResponse> projections)
        {
            Projections = projections;
        }
    }

    public class ProjectionResponse
    {
        public ConnectedProjectionName ProjectionName { get; set; }
        public ProjectionState ProjectionState { get; set; }
        public long CurrentPosition { get; set; }
        public string ErrorMessage { get; set; }

        public ProjectionResponse(RegisteredConnectedProjection projection)
        {
            ProjectionName = projection.Name;
            ProjectionState = MapProjectionState(projection.State);
            CurrentPosition = -1;
        }

        private ProjectionState MapProjectionState(ConnectedProjectionState projectionState)
        {
            return projectionState switch
            {
                ConnectedProjectionState.CatchingUp => ProjectionState.CatchingUp,
                ConnectedProjectionState.Stopped => ProjectionState.Stopped,
                ConnectedProjectionState.Subscribed => ProjectionState.Subscribed,
                _ => ProjectionState.Subscribed
            };
        }
    }

    public enum ProjectionState
    {
        Subscribed,
        CatchingUp,
        Stopped,
        Crashed,
    }
}
