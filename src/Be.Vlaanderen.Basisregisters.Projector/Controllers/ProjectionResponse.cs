namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System.Collections.Generic;
    using ConnectedProjections;
    using ProjectionHandling.Runner.ProjectionStates;

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

        public ProjectionResponse(RegisteredConnectedProjection projection, ProjectionStateItem? projectionState)
        {
            ProjectionName = projection.Name;
            ProjectionState = MapProjectionState(projection.State, !string.IsNullOrEmpty(projectionState?.ErrorMessage));
            CurrentPosition = projectionState?.Position ?? -1;
            ErrorMessage = projectionState?.ErrorMessage ?? string.Empty;
        }

        private ProjectionState MapProjectionState(ConnectedProjectionState projectionState, bool hasErrorMessage)
        {
            return projectionState switch
            {
                ConnectedProjectionState.Stopped => hasErrorMessage ? ProjectionState.Crashed : ProjectionState.Stopped,
                ConnectedProjectionState.CatchingUp => ProjectionState.CatchingUp,
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
