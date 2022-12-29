namespace Be.Vlaanderen.Basisregisters.Projector.Tests.StreamGapStrategies
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Internal;
    using Internal.Configuration;
    using Internal.Exceptions;
    using Internal.StreamGapStrategies;
    using Moq;
    using SqlStreamStore.Streams;
    using Xunit;

    public class When_handling_a_message_using_the_default_subscription_stream_gap_strategy
    {
        private readonly Func<Task> _handlingMessage;
        private readonly ConnectedProjectionIdentifier _projection;
        private readonly IEnumerable<long> _missingPositions;
        private string _processMessageFunctionStatus;


        public When_handling_a_message_using_the_default_subscription_stream_gap_strategy()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionIdentifiers();

            _projection = fixture.Create<ConnectedProjectionIdentifier>();
            _missingPositions = fixture.CreateMany<long>(1, 10);
            var message = fixture.Create<StreamMessage>();

            var stateMock = new Mock<IProcessedStreamState>();
            stateMock
                .Setup(state => state.DetermineGapPositions(message))
                .Returns(_missingPositions);

            _processMessageFunctionStatus = "NotExecuted";

            _handlingMessage = async () => await new DefaultSubscriptionStreamGapStrategy(Mock.Of<IStreamGapStrategyConfigurationSettings>())
                .HandleMessage(
                    message,
                    stateMock.Object,
                    (_, token) =>
                    {
                        _processMessageFunctionStatus = "Executed";
                        return Task.CompletedTask;
                    },
                    _projection,
                    fixture.Create<CancellationToken>());
        }

        [Fact]
        public async Task Then_a_detected_stream_gap_exception_is_thrown()
        {
            var exception = await _handlingMessage
                .Should()
                .ThrowAsync<StreamGapDetectedException>()
                .WithMessage(_projection.ToString())
                .WithMessage($"[{string.Join(',', _missingPositions)}]");
        }


        [Fact]
        public async Task Then_process_message_should_not_be_executed()
        {
            try
            {
                await _handlingMessage();
            }
            catch { }
            finally
            {
                _processMessageFunctionStatus
                    .Should()
                    .Be("NotExecuted");
            }
        }
    }
}
