namespace Be.Vlaanderen.Basisregisters.Projector.Tests.RetryPolicy
{
    using FluentAssertions;
    using Internal;
    using Internal.RetryPolicies;
    using Moq;
    using Xunit;

    public class WhenApplyingNoRetriesPolicyOnAHandler
    {
        private readonly IConnectedProjectionMessageHandler _handlerWithoutPolicy;
        private IConnectedProjectionMessageHandler SUT { get; }

        public WhenApplyingNoRetriesPolicyOnAHandler()
        {
            _handlerWithoutPolicy = new Mock<IConnectedProjectionMessageHandler>().Object;

            SUT = new NoRetries().ApplyOn(_handlerWithoutPolicy);
        }

        [Fact]  
        public void ThenTheHandlerIsUnChanged()
        {
            SUT.Should().BeSameAs(_handlerWithoutPolicy);
        }
    }
}
