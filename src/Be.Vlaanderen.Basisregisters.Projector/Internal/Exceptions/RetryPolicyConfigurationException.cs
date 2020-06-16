namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Exceptions
{
    using System;
    using System.Configuration;

    internal class RetryPolicyConfigurationException : ConfigurationErrorsException
    {
        public RetryPolicyConfigurationException(string policyName)
            : base(GetErrorMessageForPolicy(policyName))
        { }

        public RetryPolicyConfigurationException(string policyName, Exception innerException)
            : base(GetErrorMessageForPolicy(policyName), innerException)
        { }

        private static string GetErrorMessageForPolicy(string policyName)
            => $"Could not configure RetryPolicy for {policyName}";
    }
}