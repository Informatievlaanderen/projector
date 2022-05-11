namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System;
    using Autofac;
    using Autofac.Builder;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Internal;
    using Internal.Commands;
    using Internal.Runners;
    using MessageHandling.Kafka.Simple;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterStreamStoreProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            StreamStoreConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterStreamStoreProjections<TConnectedProjection, TContext>(container => new TConnectedProjection(), settings);

        public static ContainerBuilder RegisterStreamStoreProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<TConnectedProjection> projectionFactory,
            StreamStoreConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterStreamStoreProjections<TConnectedProjection, TContext>(container => projectionFactory(), settings);

        public static ContainerBuilder RegisterStreamStoreProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<IComponentContext, TConnectedProjection> projectionFactory,
            StreamStoreConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
        {
            builder
                .Register(container =>
                    new StreamStoreConnectedProjection<TConnectedProjection, TContext>(
                        container.Resolve<Func<Owned<IConnectedProjectionContext<TContext>>>>(),
                        projectionFactory(container),
                        settings,
                        container.Resolve<ILoggerFactory>()))
                .As<IConnectedProjection>()
                .IfConcreteTypeIsNotRegistered();

            return builder;
        }

        public static ContainerBuilder RegisterKafkaProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            KafkaConnectedProjectionSettings settings,
            KafkaOptions kafkaOptions,
            ConsumerOptions consumerOptions)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterKafkaProjections<TConnectedProjection, TContext>(container => new TConnectedProjection(), settings, kafkaOptions, consumerOptions);

        public static ContainerBuilder RegisterKafkaProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<TConnectedProjection> projectionFactory,
            KafkaConnectedProjectionSettings settings,
            KafkaOptions kafkaOptions,
            ConsumerOptions consumerOptions)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterKafkaProjections<TConnectedProjection, TContext>(container => projectionFactory(), settings, kafkaOptions, consumerOptions);

        public static ContainerBuilder RegisterKafkaProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<IComponentContext, TConnectedProjection> projectionFactory,
            KafkaConnectedProjectionSettings settings,
            KafkaOptions kafkaOptions,
            ConsumerOptions consumerOptions)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
        {
            builder.Register(container=>
                    new ConnectedProjectionsKafkaSubscription<TContext>(
                        kafkaOptions,
                        consumerOptions,
                        container.Resolve<IConnectedProjectionsCommandBus>(),
                        container.Resolve<ILoggerFactory>()))
                .As<IConnectedProjectionsKafkaSubscription<TContext>>()
                .SingleInstance();

            builder.RegisterType<KafkaConnectedProjectionsSubscriptionRunner<TContext>>()
                .As<IConnectedProjectionsKafkaSubscriptionRunner<TContext>>()
                .As<IConnectedProjectionsSubscriptionRunner>()
                .SingleInstance();

            builder
                .Register(container =>
                    new KafkaConnectedProjection<TConnectedProjection, TContext>(
                        container.Resolve<Func<Owned<KafkaConnectedProjectionContext<TContext>>>>(),
                        projectionFactory(container),
                        settings,
                        container.Resolve<ILoggerFactory>()))
                .As<IConnectedProjection>()
                .IfConcreteTypeIsNotRegistered();

            return builder;
        }

        public static ContainerBuilder RegisterProjectionMigrator<TContextMigrationFactory>(this ContainerBuilder builder, IConfiguration configuration, ILoggerFactory loggerFactory)
            where TContextMigrationFactory : IRunnerDbContextMigratorFactory, new()
        {
            builder
                .RegisterInstance(new TContextMigrationFactory().CreateMigrator(configuration, loggerFactory))
                .As<IRunnerDbContextMigrator>();

            return builder;
        }

        private static void IfConcreteTypeIsNotRegistered<T>(this IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> builder)
        {
            builder
                .AsSelf()
                .IfNotRegistered(typeof(T));
        }
    }
}
