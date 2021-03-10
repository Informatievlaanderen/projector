namespace Be.Vlaanderen.Basisregisters.Projector.Controllers
{
    using System;
    using System.Collections.Generic;
    using ConnectedProjections;
    using Microsoft.AspNetCore.Http;
    using ProjectionHandling.Runner.ProjectionStates;

    public class ProjectionResponseList
    {
        public IEnumerable<ProjectionResponse> Projections { get; set; }
        public long StreamPosition { get; set; }
        public List<HateoasLink> Links { get; set; }

        public ProjectionResponseList(
            IEnumerable<ProjectionResponse> projections,
            string baseUri)
        {
            Projections = projections;
            Links = new List<HateoasLink>
            {
                new HateoasLink(new Uri($"{baseUri}/projections"), "self", HttpMethods.Get),
                new HateoasLink(new Uri($"{baseUri}/projections/start/all"), "projections", HttpMethods.Post),
                new HateoasLink(new Uri($"{baseUri}/projections/stop/all"), "projections", HttpMethods.Post)
            };
        }
    }

    public class ProjectionResponse
    {
        public ConnectedProjectionIdentifier Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectionState State { get; set; }
        public long CurrentPosition { get; set; }
        public string ErrorMessage { get; set; }
        public List<HateoasLink> Links { get; set; }

        public ProjectionResponse(
            RegisteredConnectedProjection projection,
            ProjectionStateItem? projectionState,
            string baseUri)
        {
            Id = projection.Id;
            Name = projection.Name;
            Description = projection.Description;
            State = MapProjectionState(projection.State, !string.IsNullOrEmpty(projectionState?.ErrorMessage));
            CurrentPosition = projectionState?.Position ?? -1;
            ErrorMessage = projectionState?.ErrorMessage ?? string.Empty;

            Links = new List<HateoasLink>
            {
                new HateoasLink(new Uri($"{baseUri}/projections/start/{projection.Id}"), "projections", HttpMethods.Post),
                new HateoasLink(new Uri($"{baseUri}/projections/stop/{projection.Id}"), "projections", HttpMethods.Post),
            };
        }

        private static ProjectionState MapProjectionState(ConnectedProjectionState projectionState, bool hasErrorMessage)
        {
            return projectionState switch
            {
                ConnectedProjectionState.Stopped => hasErrorMessage ? ProjectionState.Crashed : ProjectionState.Stopped,
                ConnectedProjectionState.CatchingUp => ProjectionState.CatchingUp,
                ConnectedProjectionState.Subscribed => ProjectionState.Subscribed,
                _ => ProjectionState.Unknown
            };
        }
    }

    public enum ProjectionState
    {
        Unknown,
        Subscribed,
        CatchingUp,
        Stopped,
        Crashed,
    }
}
