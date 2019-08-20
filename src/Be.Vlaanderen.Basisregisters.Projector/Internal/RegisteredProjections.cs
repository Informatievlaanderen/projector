namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using ConnectedProjections;
    using Extensions;
    using ProjectionHandling.Runner;

    internal interface IRegisteredProjections
    {
        Func<ConnectedProjectionName, bool> IsCatchingUp { set; }
        Func<ConnectedProjectionName, bool> IsSubscribed { set; }
        IEnumerable<ConnectedProjectionName> Names { get; }
        IEnumerable<IConnectedProjection> Projections { get; }
        ConnectedProjectionName GetName(string name);
        IConnectedProjection GetProjection(ConnectedProjectionName projectionName);
        bool IsProjecting(ConnectedProjectionName projectionName);
        IEnumerable<RegisteredConnectedProjection> GetStates();
    }

    internal class RegisteredProjections : IRegisteredProjections
    {
        private readonly IEnumerable<IConnectedProjection> _registeredProjections;

        public Func<ConnectedProjectionName, bool> IsCatchingUp { private get; set; }

        public Func<ConnectedProjectionName, bool> IsSubscribed { private get; set; }

        public RegisteredProjections(IEnumerable<IConnectedProjection> registeredProjections)
            => _registeredProjections = registeredProjections?.RemoveNullReferences() ?? throw new ArgumentNullException(nameof(registeredProjections));

        public IEnumerable<ConnectedProjectionName> Names =>
            _registeredProjections
                .Select(projection => projection.Name);

        public IEnumerable<IConnectedProjection> Projections => _registeredProjections;

        public ConnectedProjectionName GetName(string name) =>
            _registeredProjections
                ?.SingleOrDefault(projection => projection.Name.Equals(name))
                ?.Name;

        public IConnectedProjection GetProjection(ConnectedProjectionName projectionName) =>
            _registeredProjections
                ?.SingleOrDefault(projection => projection.Name.Equals(projectionName));

        public bool IsProjecting(ConnectedProjectionName projectionName) =>
            GetState(projectionName) != ConnectedProjectionState.Stopped;

        public IEnumerable<RegisteredConnectedProjection> GetStates() => _registeredProjections
            .Select(projection =>
                new RegisteredConnectedProjection(
                    projection.Name,
                    GetState(projection.Name)));

        private ConnectedProjectionState GetState(ConnectedProjectionName projectionName)
        {
            if (IsCatchingUp?.Invoke(projectionName) ?? false)
                return ConnectedProjectionState.CatchingUp;

            if (IsSubscribed?.Invoke(projectionName) ?? false)
                return ConnectedProjectionState.Subscribed;

            return ConnectedProjectionState.Stopped;
        }
    }
}
