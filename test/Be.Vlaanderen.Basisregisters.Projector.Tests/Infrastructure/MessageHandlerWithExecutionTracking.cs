namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Assertions;
    using ConnectedProjections;
    using Internal;
    using Internal.StreamGapStrategies;
    using Microsoft.Extensions.Logging;
    using Moq;
    using SqlStreamStore.Streams;

    internal class MessageHandlerWithExecutionTracking : IStreamStoreConnectedProjectionMessageHandler
    {
        private readonly Stack<Exception> _exceptionSequence;
        
        public ConnectedProjectionIdentifier Projection { get; }
        public ILogger Logger { get; }

        public MessageHandlerWithExecutionTracking(
            ConnectedProjectionIdentifier projection,
            ILogger logger,
            params Exception[] exceptionSequence)
        {
            Projection = projection;
            Logger = logger;
            _exceptionSequence = new Stack<Exception>(exceptionSequence.Reverse());
        }

        public Task HandleAsync(
            IEnumerable<StreamMessage> messages,
            IStreamGapStrategy streamGapStrategy,
            CancellationToken cancellationToken)
        {
            _executions.Add(CreateExecutionId(messages));

            if (_exceptionSequence.Any())
                throw _exceptionSequence.Pop();

            return Task.CompletedTask;
        }

        private readonly List<string> _executions = new List<string>();

        private static string CreateExecutionId(IEnumerable<StreamMessage> messages)
            => string.Join('|', messages.Select(message => message.MessageId));
        
        public void VerifyExecuted(IEnumerable<StreamMessage> messages, Times times)
        {
            var messageList = messages.ToList();
            var executionId = CreateExecutionId(messageList);
            var numberOfExecutions = _executions.Count(execution => executionId == execution);

            if (!times.Validate(numberOfExecutions))
                throw new MessageHandlingInvalidNumberOfExecutions(messageList, times, numberOfExecutions);
        }
    }
}
