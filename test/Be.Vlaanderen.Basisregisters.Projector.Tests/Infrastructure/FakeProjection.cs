namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Internal;
    using Internal.StreamGapStrategies;
    using Moq;
    using ProjectionHandling.Runner;
    using ProjectionHandling.Runner.ProjectionStates;
    using SqlStreamStore.Streams;

    internal class FakeProjection : IConnectedProjection<FakeProjectionContext>, IConnectedProjection
    {
        public ConnectedProjectionName Name { get; }
        public dynamic Instance => this;
        public IConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }
        public Func<Owned<IConnectedProjectionContext<FakeProjectionContext>>> ContextFactory { get; }

        public FakeProjection(
            string name,
            Func<IEnumerable<StreamMessage>, IStreamGapStrategy, ConnectedProjectionName, CancellationToken, Task> messageHandler,
            IConnectedProjectionContext<FakeProjectionContext> context)
        {
            Name = new ConnectedProjectionName($"{GetType().FullName}-{name}");

            var messageHandlerMock = new Mock<IConnectedProjectionMessageHandler>();
            messageHandlerMock
                .SetupGet(handler => handler.RunnerName)
                .Returns(Name);
            messageHandlerMock
                .Setup(handler => handler.HandleAsync(It.IsAny<IEnumerable<StreamMessage>>(), It.IsAny<IStreamGapStrategy>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<StreamMessage> messages, IStreamGapStrategy strategy, CancellationToken ct) => messageHandler(messages, strategy, Name, ct));

            ConnectedProjectionMessageHandler = messageHandlerMock.Object;
            ContextFactory = () => new Owned<IConnectedProjectionContext<FakeProjectionContext>>(context, Mock.Of<IDisposable>());
        }

        public Task UpdateUserDesiredState(UserDesiredState userDesiredState, CancellationToken cancellationToken)
            => throw new NotImplementedException($"{nameof(FakeProjection)}.{nameof(UpdateUserDesiredState)}");

        public Task<bool> ShouldResume(CancellationToken cancellationToken)
            => throw new NotImplementedException($"{nameof(FakeProjection)}.{nameof(ShouldResume)}");

        public Task<ProjectionStateItem?> GetProjectionState(CancellationToken cancellationToken)
            => throw new NotImplementedException($"{nameof(FakeProjection)}.{nameof(GetProjectionState)}");

        public Task SetErrorMessage(Exception exception, CancellationToken cancellationToken)
            => throw new NotImplementedException($"{nameof(FakeProjection)}.{nameof(SetErrorMessage)}");

        public Task ClearErrorMessage(CancellationToken cancellationToken)
            => throw new NotImplementedException($"{nameof(FakeProjection)}.{nameof(ClearErrorMessage)}");
    }

    public class FakeProjectionContext : RunnerDbContext<FakeProjectionContext>
    {
        public override string ProjectionStateSchema => "fake-context-schema";
    }

}
