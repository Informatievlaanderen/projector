namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using Infrastructure;
    using Internal.Commands;
    using Internal.Commands.Subscription;
    using Internal.Runners;
    using Microsoft.Extensions.Logging;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using SqlStreamStore.Subscriptions;
    using Xunit;

    public class ConnectedProjectionsStreamStoreSubscriptionTests
    {
        private readonly ConnectedProjectionsStreamStoreSubscription _sut;
        private readonly Mock<IConnectedProjectionsCommandBus> _commandBusMock;
        private readonly FakeLogger _loggerMock;
        private readonly StreamStoreMock _streamStoreMock;
        private readonly IFixture _fixture;

        public ConnectedProjectionsStreamStoreSubscriptionTests()
        {
            _fixture = new Fixture();

            _streamStoreMock = new StreamStoreMock();
            _commandBusMock = new Mock<IConnectedProjectionsCommandBus>();

            var loggerFactory = new FakeLoggerFactory();
            _loggerMock = loggerFactory.ResolveLoggerMock<ConnectedProjectionsStreamStoreSubscription>();

            _sut = new ConnectedProjectionsStreamStoreSubscription(
                _streamStoreMock.Object,
                _commandBusMock.Object,
                loggerFactory);
        }

        [Fact]
        public async Task When_the_subscription_is_started_then_the_subscription_is_running_returns_true()
        {
            await _sut.Start();

            _sut.StreamIsRunning
                .Should()
                .BeTrue();
        }

        [Fact]
        public async Task When_the_subscription_is_started_then_it_was_started_at_the_head_position()
        {
            var headPosition = _fixture.Create<uint>();
            _streamStoreMock.SetHeadPosition(headPosition);
            await _sut.Start();
            
            _streamStoreMock.VerifyStreamSubscribedAfterPosition(headPosition);
        }

        [Fact]
        public async Task When_the_subscription_is_started_then_start_position_was_logged()
        {
            var headPosition = _fixture.Create<uint>();
            _streamStoreMock.SetHeadPosition(headPosition);
            await _sut.Start();
            
            _loggerMock.Verify(LogLevel.Information, $"Started subscription stream after {headPosition}", Times.Once);
        }
        
        [Fact]
        public async Task When_the_subscription_is_started_with_the_head_position_before_zero_then_the_subscription_was_subscribed_without_a_start_position()
        {
            _streamStoreMock.SetHeadPosition(-1);
            await _sut.Start();

            _streamStoreMock.VerifyStreamSubscribedAfterPosition(null);
        }
        
        [Fact]
        public async Task When_a_running_subscription_is_dropped_then_the_subscription_is_running_returns_false()
        {
            await _sut.Start();

            _streamStoreMock.DropSubscription(_fixture.Create<SubscriptionDroppedReason>(), _fixture.Create<Exception>());

            _sut.StreamIsRunning
                .Should()
                .BeFalse();
        }

        [Fact]
        public async Task When_a_running_subscription_is_dropped_with_an_exception_then_an_error_is_logged()
        {
            await _sut.Start();
            var droppedReason = _fixture.Create<SubscriptionDroppedReason>();
            var exception = new Exception(_fixture.Create<string>());
            var streamName = _sut.StreamName;

            _streamStoreMock.DropSubscription(droppedReason, exception);

            SubscriptionDroppedErrorLogged(streamName, droppedReason, exception, Times.Once);
        }

        [Fact]
        public async Task When_a_running_subscription_is_dropped_without_an_exception_then_no_error_is_logged()
        {
            await _sut.Start();
            var droppedReason = _fixture.Create<SubscriptionDroppedReason>();
            var streamName = _sut.StreamName;

            _streamStoreMock.DropSubscription(droppedReason, null);

            SubscriptionDroppedErrorLogged(streamName, droppedReason, null, Times.Never);
        }

        [Fact]
        public async Task When_a_running_subscription_is_dropped_with_a_task_cancelled_exception_then_no_error_is_logged()
        {
            await _sut.Start();
            var droppedReason = _fixture.Create<SubscriptionDroppedReason>();
            var canceledException = _fixture.Create<TaskCanceledException>();
            var streamName = _sut.StreamName;

            _streamStoreMock.DropSubscription(droppedReason, canceledException);

            SubscriptionDroppedErrorLogged(streamName, droppedReason, canceledException, Times.Never);
        }

        [Fact]
        public async Task When_the_subscription_receives_a_message_then_the_process_message_command_gets_queued()
        {
            await _sut.Start();
            var message = _fixture.Create<StreamMessage>();
            var cancellationToken = _fixture.Create<CancellationToken>();

            _streamStoreMock.PushMessage(message, cancellationToken);

            _commandBusMock.Verify(
                bus => bus.Queue(It.Is<ProcessStreamEvent>(command =>
                    command.Message.Equals(message)
                    && command.CancellationToken == cancellationToken)),
                Times.Once);
        }


        private void SubscriptionDroppedErrorLogged(
            string streamName,
            SubscriptionDroppedReason reason,
            Exception exception,
            Func<Times> times)
            => _loggerMock.Verify($"Subscription {streamName} was dropped. Reason: {reason}", exception, times);

        private class StreamStoreMock : Mock<IReadonlyStreamStore>
        {
            private AllStreamMessageReceived _streamMessageReceived;
            private AllSubscriptionDropped _subscriptionDropped;
            private IAllStreamSubscription _allStreamSubscription;

            public StreamStoreMock()
            {
                SetHeadPosition(-1);

                Setup(store => store.SubscribeToAll(
                        It.IsAny<long?>(),
                        It.IsAny<AllStreamMessageReceived>(),
                        It.IsAny<AllSubscriptionDropped>(),
                        It.IsAny<HasCaughtUp>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>()))
                    .Returns((
                        long? continueAfterPosition,
                        AllStreamMessageReceived allStreamMessageReceived,
                        AllSubscriptionDropped allSubscriptionDropped,
                        HasCaughtUp hasCaughtUp,
                        bool prefetchJsonData,
                        string name)
                        => SubscribeToAll(allStreamMessageReceived, allSubscriptionDropped));
            }

            public void VerifyStreamSubscribedAfterPosition(long? position)
            {
                Verify(store => store.SubscribeToAll(
                    position,
                    It.IsAny<AllStreamMessageReceived>(),
                    It.IsAny<AllSubscriptionDropped>(),
                    It.IsAny<HasCaughtUp>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()));
            }

            public void SetHeadPosition(long value)
            {
                Setup(store => store.ReadHeadPosition(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(value));
            }

            private IAllStreamSubscription SubscribeToAll(
                AllStreamMessageReceived streamMessageReceived,
                AllSubscriptionDropped subscriptionDropped)
            {
                _streamMessageReceived = streamMessageReceived;
                _subscriptionDropped = subscriptionDropped;

                var mock = new Mock<IAllStreamSubscription>();
                mock.Setup(subscription => subscription.Name).Returns($"AllStreamSubscription<{Guid.NewGuid()}>");
                _allStreamSubscription = mock.Object;

                return _allStreamSubscription;
            }

            public void PushMessage(StreamMessage message, CancellationToken cancellationToken)
            {
                _streamMessageReceived(_allStreamSubscription, message, cancellationToken);
            }

            public void DropSubscription(SubscriptionDroppedReason reason, Exception exception)
            {
                _subscriptionDropped(_allStreamSubscription, reason, exception);
            }

        }
    }
}
