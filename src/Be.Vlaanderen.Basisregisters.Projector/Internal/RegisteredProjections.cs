namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConnectedProjections;
    using Extensions;

    internal interface IRegisteredProjections
    {
        IEnumerable<IConnectedProjection> Projections { get; }
        IEnumerable<ConnectedProjectionIdentifier> Identifiers { get; }
        Func<ConnectedProjectionIdentifier, bool> IsCatchingUp { set; }
        Func<ConnectedProjectionIdentifier, bool> IsSubscribed { set; }
        bool Exists(ConnectedProjectionIdentifier projection);
        IConnectedProjection? GetProjection(ConnectedProjectionIdentifier projection);
        bool IsProjecting(ConnectedProjectionIdentifier projection);
        IEnumerable<RegisteredConnectedProjection> GetStates();
    }

    internal class RegisteredProjections : IRegisteredProjections
    {
        public IEnumerable<IConnectedProjection> Projections { get; }

        public IEnumerable<ConnectedProjectionIdentifier> Identifiers
            => Projections.Select(projection => projection.Id);

        public Func<ConnectedProjectionIdentifier, bool>? IsCatchingUp { private get; set; }

        public Func<ConnectedProjectionIdentifier, bool>? IsSubscribed { private get; set; }

        public RegisteredProjections(IEnumerable<IConnectedProjection> registeredProjections)
            => Projections = registeredProjections?.RemoveNullReferences() ?? throw new ArgumentNullException(nameof(registeredProjections));
        
        public bool Exists(ConnectedProjectionIdentifier projection)
            => Projections.Any(registeredProjection => registeredProjection.Id == projection);

        public IConnectedProjection? GetProjection(ConnectedProjectionIdentifier projection) 
            => Projections.SingleOrDefault(registeredProjection => registeredProjection.Id == projection);

        public bool IsProjecting(ConnectedProjectionIdentifier projection)
            => GetState(projection) != ConnectedProjectionState.Stopped;

        public IEnumerable<RegisteredConnectedProjection> GetStates() => Projections
            .Select(projection =>
                new RegisteredConnectedProjection(
                    projection.Id,
                    GetState(projection.Id),
                    projection.GetName(),
                    projection.GetDescription()));

        private ConnectedProjectionState GetState(ConnectedProjectionIdentifier projection)
        {
            if (IsCatchingUp?.Invoke(projection) ?? false)
                return ConnectedProjectionState.CatchingUp;

            if (IsSubscribed?.Invoke(projection) ?? false)
                return ConnectedProjectionState.Subscribed;

            return ConnectedProjectionState.Stopped;
        }
    }
}
