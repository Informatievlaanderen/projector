namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using System.Reflection;

    public class ConnectedProjectionName
    {
        private readonly string _name;

        internal ConnectedProjectionName(MemberInfo connectedProjectionType)
        {
            _name = connectedProjectionType?.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConnectedProjectionName);
        }

        public bool Equals(ConnectedProjectionName other)
        {
            return null != other && Equals(other._name);
        }

        public bool Equals(string other)
        {
            return string.Equals(_name, other, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return _name?.ToLowerInvariant().GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
