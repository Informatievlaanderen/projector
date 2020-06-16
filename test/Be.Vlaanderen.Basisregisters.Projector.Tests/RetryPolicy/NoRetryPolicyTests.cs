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
        private readonly IConnectedProjectionMessageHandler _sut;

        public WhenApplyingNoRetriesPolicyOnAHandler()
        {
            _handlerWithoutPolicy = new Mock<IConnectedProjectionMessageHandler>().Object;

            _sut = new NoRetries().ApplyOn(_handlerWithoutPolicy);
        }

        [Fact]  
        public void ThenTheHandlerIsUnChanged()
        {
            _sut.Should().BeSameAs(_handlerWithoutPolicy);
        }
    }
}
