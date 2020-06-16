namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System;
    using ConnectedProjections;
    using Exceptions;
    using Microsoft.Extensions.Configuration;

    internal static class RetryPolicyConfigurationExtensions
    {
        public static MessageHandlingRetryPolicy Configure(
            this IConfiguration configuration,
            Func<int, TimeSpan, MessageHandlingRetryPolicy> policyFactory,
            string policyName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrWhiteSpace(policyName))
                throw new ArgumentNullException(nameof(policyName));
            try
            {
                var policyConfig = configuration
                    .GetSection("RetryPolicies")
                    .GetSection(policyName);

                var retries = policyConfig.GetValue<int>("NumberOfRetries");
                var delay = TimeSpan.FromSeconds(policyConfig.GetValue<int>("DelayInSeconds"));
                return policyFactory(retries, delay) ?? throw new RetryPolicyConfigurationException(policyName);
            }
            catch (RetryPolicyConfigurationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw new RetryPolicyConfigurationException(policyName, exception);
            }
        }
    }
}
