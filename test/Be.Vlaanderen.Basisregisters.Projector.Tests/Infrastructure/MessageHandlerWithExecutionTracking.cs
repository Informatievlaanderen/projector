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
    using Microsoft.Extensions.Logging;
    using Moq;
    using SqlStreamStore.Streams;

    public class MessageHandlerWithExecutionTracking : IConnectedProjectionMessageHandler
    {
        private readonly Stack<Exception> _exceptionSequence;
        
        public ConnectedProjectionName RunnerName { get; }
        public ILogger Logger { get; }

        public MessageHandlerWithExecutionTracking(
            ConnectedProjectionName runnerName,
            ILogger logger,
            params Exception[] exceptionSequence)
        {
            RunnerName = runnerName;
            Logger = logger;
            _exceptionSequence = new Stack<Exception>(exceptionSequence.Reverse());
        }

        public Task HandleAsync(IEnumerable<StreamMessage> messages, CancellationToken cancellationToken)
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
