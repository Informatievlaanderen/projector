namespace Be.Vlaanderen.Basisregisters.Projector.Tests.RetryPolicy
{
    using FluentAssertions;
    using Internal;
    using Internal.RetryPolicies;
    using Moq;
    using Xunit;

    public class WhenApplyingNoRetriesPolicyOnAHandler
    {
        private readonly IStreamStoreConnectedProjectionMessageHandler _handlerWithoutPolicy;
        private readonly IStreamStoreConnectedProjectionMessageHandler _sut;

        public WhenApplyingNoRetriesPolicyOnAHandler()
        {
            _handlerWithoutPolicy = new Mock<IStreamStoreConnectedProjectionMessageHandler>().Object;

            _sut = new StreamStoreNoRetries().ApplyOn(_handlerWithoutPolicy);
        }

        [Fact]  
        public void ThenTheHandlerIsUnChanged()
        {
            _sut.Should().BeSameAs(_handlerWithoutPolicy);
        }
    }
}
