namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System;
    using ConnectedProjections;
    using Exceptions;
    using Microsoft.Extensions.Configuration;

    internal static class RetryPolicyConfigurationExtensions
    {
        public static IHandlingRetryPolicy Configure(
            this IConfiguration configuration,
            Func<int, TimeSpan, IHandlingRetryPolicy> policyFactory,
            Func<IConfigurationSection, int> getInt,
            Func<IConfigurationSection, TimeSpan> getTimeSpan,
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

                return policyFactory(getInt(policyConfig), getTimeSpan(policyConfig)) ?? throw new RetryPolicyConfigurationException(policyName);
            }
            catch (RetryPolicyConfigurationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new RetryPolicyConfigurationException(policyName, exception);
            }
        }
    }
}
