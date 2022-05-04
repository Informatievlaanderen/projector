namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Internal;
    using Internal.Commands;
    using Internal.StreamGapStrategies;
    using Microsoft.Extensions.Logging;
    using Moq;
    using ProjectionHandling.Runner;
    using ProjectionHandling.Runner.ProjectionStates;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    internal class FakeProjection : IStreamStoreConnectedProjection<FakeProjectionContext>, IConnectedProjection
    {
        public ConnectedProjectionIdentifier Id { get; }
        public ConnectedProjectionInfo Info { get; }
        public dynamic Instance => this;
        public IStreamStoreConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }

        public ConnectedProjectionCatchUp<FakeProjectionContext> CreateCatchUp(
            IReadonlyStreamStore streamStore,
            IConnectedProjectionsCommandBus commandBus,
            IStreamGapStrategy catchUpStreamGapStrategy,
            ILogger logger)
            => throw new NotImplementedException();

        public Func<Owned<IConnectedProjectionContext<FakeProjectionContext>>> ContextFactory { get; }

        public FakeProjection(
            string id,
            Func<IEnumerable<StreamMessage>, IStreamGapStrategy, ConnectedProjectionIdentifier, CancellationToken, Task> messageHandler,
            IConnectedProjectionContext<FakeProjectionContext> context)
        {
            Id = new ConnectedProjectionIdentifier($"{GetType().FullName}-{id}");
            Info = new ConnectedProjectionInfo(string.Empty, string.Empty);

            var messageHandlerMock = new Mock<IStreamStoreConnectedProjectionMessageHandler>();
            messageHandlerMock
                .SetupGet(handler => handler.Projection)
                .Returns(Id);
            messageHandlerMock
                .Setup(handler => handler.HandleAsync(It.IsAny<IEnumerable<StreamMessage>>(), It.IsAny<IStreamGapStrategy>(), It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<StreamMessage> messages, IStreamGapStrategy strategy, CancellationToken ct) => messageHandler(messages, strategy, Id, ct));

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
