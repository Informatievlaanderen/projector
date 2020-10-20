namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using ConnectedProjections;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Internal;
    using Internal.StreamGapStrategies;
    using Moq;
    using ProjectionHandling.Connector;
    using SqlStreamStore.Streams;
    using TestProjections.Projections;
    using Xunit;

    public class When_handling_a_message_with_a_position_that_does_not_follow_the_runner_position
    {
        private readonly Mock<IStreamGapStrategy> _streamGapStrategyMock;
        private readonly ConnectedProjectionName _runnerName;
        private readonly StreamMessage _message;
        private readonly long _runnerPosition;

        public When_handling_a_message_with_a_position_that_does_not_follow_the_runner_position()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();
            
            _runnerName = fixture.Create<ConnectedProjectionName>();
            _runnerPosition = fixture
                .CreatePositive<long>()
                .WithMaximumValueOf(long.MaxValue - 100);

            var contextMock = new Mock<IConnectedProjectionContext<ProjectionContext>>();
            contextMock
                .Setup(context => context.GetProjectionPosition(_runnerName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_runnerPosition);
                
            var sut = new ConnectedProjectionMessageHandler<ProjectionContext>(
                _runnerName,
                new ConnectedProjectionHandler<ProjectionContext>[0],
                contextMock.CreateOwnedObject, 
                new FakeLoggerFactory()
            );

            _message = fixture
                .Build<ConfigurableStreamMessage>()
                .With(streamMessage => streamMessage.Position, (_runnerPosition + 1).CreateRandomHigherValue())
                .Create();

            _streamGapStrategyMock = new Mock<IStreamGapStrategy>();
            _streamGapStrategyMock
                .Setup(strategy => strategy.HandleMessage(
                    It.IsAny<StreamMessage>(),
                    It.IsAny<IProcessedStreamState>(),
                    It.IsAny<Func<StreamMessage, CancellationToken, Task>>(),
                    _runnerName,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            sut.HandleAsync(
                    new []{ _message },
                    _streamGapStrategyMock.Object,
                    CancellationToken.None)
                .GetAwaiter();
        }

        [Fact]
        public void Then_the_stream_gap_strategy_should_handle_the_message()
        {
            _streamGapStrategyMock.Verify(
                strategy => strategy.HandleMessage(
                    _message,
                    It.Is<IProcessedStreamState>(state => state.Position == _runnerPosition),
                    It.IsAny<Func<StreamMessage, CancellationToken, Task>>(),
                    _runnerName,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    public class When_handling_a_message_with_a_position_that_does_follow_the_runner_position
    {
        private readonly Mock<IStreamGapStrategy> _streamGapStrategyMock;
        private readonly ConnectedProjectionName _runnerName;
        private readonly long _runnerPosition;
        private readonly StreamMessage _message;

        public When_handling_a_message_with_a_position_that_does_follow_the_runner_position()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();
            
            _runnerName = fixture.Create<ConnectedProjectionName>();
            _runnerPosition = fixture
                .CreatePositive<long>()
                .WithMaximumValueOf(long.MaxValue - 100);

            var contextMock = new Mock<IConnectedProjectionContext<ProjectionContext>>();
            contextMock
                .Setup(context => context.GetProjectionPosition(_runnerName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_runnerPosition);

            var sut = new ConnectedProjectionMessageHandler<ProjectionContext>(
                _runnerName,
                new ConnectedProjectionHandler<ProjectionContext>[0],
                contextMock.CreateOwnedObject,
                new FakeLoggerFactory()
            );

            _message = fixture
                .Build<ConfigurableStreamMessage>()
                .With(streamMessage => streamMessage.Position, _runnerPosition + 1)
                .Create();

            _streamGapStrategyMock = new Mock<IStreamGapStrategy>();
            _streamGapStrategyMock
                .Setup(strategy => strategy.HandleMessage(
                    It.IsAny<StreamMessage>(),
                    It.IsAny<IProcessedStreamState>(),
                    It.IsAny<Func<StreamMessage, CancellationToken, Task>>(),
                    _runnerName,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            sut.HandleAsync(
                    new[] { _message },
                    _streamGapStrategyMock.Object,
                    CancellationToken.None)
                .GetAwaiter();
        }

        [Fact]
        public void Then_the_stream_gap_strategy_should_not_handle_the_message()
        {
            _streamGapStrategyMock.Verify(
                strategy => strategy.HandleMessage(
                    _message,
                    It.Is<IProcessedStreamState>(state => state.Position == _runnerPosition),
                    It.IsAny<Func<StreamMessage, CancellationToken, Task>>(),
                    _runnerName,
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    public class When_handling_a_message_with_a_position_that_does_not_follow_the_previous_message_position
    {
        private readonly Mock<IStreamGapStrategy> _streamGapStrategyMock;
        private readonly ConnectedProjectionName _runnerName;
        private readonly StreamMessage _message1, _message2;

        public When_handling_a_message_with_a_position_that_does_not_follow_the_previous_message_position()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();
            
            _runnerName = fixture.Create<ConnectedProjectionName>();

            var contextMock = new Mock<IConnectedProjectionContext<ProjectionContext>>();
            var runnerPosition = fixture
                .CreatePositive<long>()
                .WithMaximumValueOf(long.MaxValue - 100);
            contextMock
                .Setup(context => context.GetProjectionPosition(_runnerName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(runnerPosition);

            var sut = new ConnectedProjectionMessageHandler<ProjectionContext>(
                _runnerName,
                new ConnectedProjectionHandler<ProjectionContext>[0],
                contextMock.CreateOwnedObject,
                new FakeLoggerFactory()
            );

            _message1 = fixture
                .Build<ConfigurableStreamMessage>()
                .With(streamMessage => streamMessage.Position, runnerPosition + 1)
                .Create();

            _message2 = fixture
                .Build<ConfigurableStreamMessage>()
                .With(streamMessage => streamMessage.Position, (_message1.Position + 1).CreateRandomHigherValue())
                .Create();

            _streamGapStrategyMock = new Mock<IStreamGapStrategy>();
            _streamGapStrategyMock
                .Setup(strategy => strategy.HandleMessage(
                    It.IsAny<StreamMessage>(),
                    It.IsAny<IProcessedStreamState>(),
                    It.IsAny<Func<StreamMessage, CancellationToken, Task>>(),
                    _runnerName,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            sut.HandleAsync(
                    new[] { _message1, _message2 },
                    _streamGapStrategyMock.Object,
                    CancellationToken.None)
                .GetAwaiter();
        }

        [Fact]
        public void Then_the_stream_gap_strategy_should_handle_the_message()
        {
            _streamGapStrategyMock.Verify(
                strategy => strategy.HandleMessage(
                    _message2,
                    It.Is<IProcessedStreamState>(state => state.Position == _message1.Position),
                    It.IsAny<Func<StreamMessage, CancellationToken, Task>>(),
                    _runnerName,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    public class When_handling_a_message_with_a_position_that_does_follow_the_previous_message_position
    {
        private readonly Mock<IStreamGapStrategy> _streamGapStrategyMock;
        private readonly ConnectedProjectionName _runnerName;
        private readonly StreamMessage _message1, _message2;

        public When_handling_a_message_with_a_position_that_does_follow_the_previous_message_position()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();
            
            _runnerName = fixture.Create<ConnectedProjectionName>();

            var contextMock = new Mock<IConnectedProjectionContext<ProjectionContext>>();
            var runnerPosition = fixture
                .CreatePositive<long>()
                .WithMaximumValueOf(long.MaxValue - 100);
            
            contextMock
                .Setup(context => context.GetProjectionPosition(_runnerName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(runnerPosition);

            var sut = new ConnectedProjectionMessageHandler<ProjectionContext>(
                _runnerName,
                new ConnectedProjectionHandler<ProjectionContext>[0],
                contextMock.CreateOwnedObject,
                new FakeLoggerFactory()
            );

            _message1 = fixture
                .Build<ConfigurableStreamMessage>()
                .With(streamMessage => streamMessage.Position, runnerPosition + 1)
                .Create();

            _message2 = fixture
                .Build<ConfigurableStreamMessage>()
                .With(streamMessage => streamMessage.Position, _message1.Position + 1)
                .Create();

            _streamGapStrategyMock = new Mock<IStreamGapStrategy>();
            _streamGapStrategyMock
                .Setup(strategy => strategy.HandleMessage(
                    It.IsAny<StreamMessage>(),
                    It.IsAny<IProcessedStreamState>(),
                    It.IsAny<Func<StreamMessage, CancellationToken, Task>>(),
                    _runnerName,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            sut.HandleAsync(
                    new[] { _message1, _message2 },
                    _streamGapStrategyMock.Object,
                    CancellationToken.None)
                .GetAwaiter();
        }

        [Fact]
        public void Then_the_stream_gap_strategy_should_not_handle_the_message()
        {
            _streamGapStrategyMock.Verify(
                strategy => strategy.HandleMessage(
                    _message2,
                    It.Is<IProcessedStreamState>(state => state.Position == _message1.Position),
                    It.IsAny<Func<StreamMessage, CancellationToken, Task>>(),
                    _runnerName,
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
