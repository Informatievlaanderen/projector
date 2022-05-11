namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Commands.Subscription;
    using MessageHandling.Kafka.Simple;
    using Microsoft.Extensions.Logging;

    internal interface IConnectedProjectionsKafkaSubscription<TContext>
    {
        bool StreamIsRunning { get; }

        Task Start(CancellationToken cancellationToken);
    }

    internal class ConnectedProjectionsKafkaSubscription<TContext> : IConnectedProjectionsKafkaSubscription<TContext>
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly ConsumerOptions _consumerOptions;
        private readonly IConnectedProjectionsCommandBus _commandBus;
        private readonly ILogger<ConnectedProjectionsKafkaSubscription<TContext>> _logger;
        private Task<Result<KafkaJsonMessage>> _consumeTask;

        public bool StreamIsRunning => !_consumeTask.IsCompleted;

        public ConnectedProjectionsKafkaSubscription(
            KafkaOptions kafkaOptions,
            ConsumerOptions consumerOptions,
            IConnectedProjectionsCommandBus commandBus,
            ILoggerFactory loggerFactory)
        {
            _kafkaOptions = kafkaOptions ?? throw new ArgumentNullException(nameof(kafkaOptions));
            _consumerOptions = consumerOptions ?? throw new ArgumentNullException(nameof(consumerOptions));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _logger = loggerFactory.CreateLogger<ConnectedProjectionsKafkaSubscription<TContext>>() ??
                      throw new ArgumentNullException(nameof(loggerFactory));
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Start consumption on topic {Topic}", _consumerOptions.Topic);

            _consumeTask = KafkaConsumer.Consume(
                _kafkaOptions,
                _consumerOptions.ConsumerGroupId,
                _consumerOptions.Topic,
                async message =>
                {
                    await OnKafkaMessageReceived(message, cancellationToken);
                },
                _consumerOptions.Offset,
                cancellationToken);

            return Task.CompletedTask;
        }

        private Task OnKafkaMessageReceived(
            object message,
            CancellationToken cancellationToken)
        {
            _commandBus.Queue(new ProcessKafkaMessage(message, cancellationToken));

            return Task.CompletedTask;
        }
    }
}
