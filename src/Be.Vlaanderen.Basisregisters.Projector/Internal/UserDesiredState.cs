namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;

    public class UserDesiredState : IEquatable<UserDesiredState>
    {
        private readonly string _value;

        public static readonly UserDesiredState Started = new UserDesiredState(nameof(Started));
        public static readonly UserDesiredState Stopped = new UserDesiredState(nameof(Stopped));

        public static readonly UserDesiredState[] All =
        {
            Started,
            Stopped
        };

        private UserDesiredState(string value) => _value = value;

        public static bool CanParse(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return Array.Find(All, candidate => candidate._value == value) != null;
        }

        public static bool TryParse(string value, out UserDesiredState parsed)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static UserDesiredState Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!TryParse(value, out var parsed))
                throw new FormatException($"The identifier {value} does not correspond to any alternative label types.");

            return parsed;
        }

        public bool Equals(UserDesiredState other) => other != null && other._value == _value;
        public override bool Equals(object? obj) => obj is UserDesiredState type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;

        public static implicit operator string(UserDesiredState instance) => instance.ToString();
        public static bool operator ==(UserDesiredState left, UserDesiredState right) => Equals(left, right);
        public static bool operator !=(UserDesiredState left, UserDesiredState right) => !Equals(left, right);
    }
}
