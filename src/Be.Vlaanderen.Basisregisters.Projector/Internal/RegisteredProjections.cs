namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConnectedProjections;
    using Extensions;

    internal class RegisteredProjections
    {
        private readonly IEnumerable<IConnectedProjection> _registeredProjections;

        public Func<ConnectedProjectionName, bool> IsCatchingUp { private get; set; }

        public Func<ConnectedProjectionName, bool> IsSubscribed { private get; set; }

        public RegisteredProjections(IEnumerable<IConnectedProjection> registeredProjections)
            => _registeredProjections = registeredProjections?.RemoveNullReferences() ?? throw new ArgumentNullException(nameof(registeredProjections));

        public IEnumerable<ConnectedProjectionName> Names =>
            _registeredProjections
                .Select(projection => projection.Name);

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
