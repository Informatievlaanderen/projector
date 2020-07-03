namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System;
    using Autofac;
    using Autofac.Builder;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;

    public static class ContainerBuilderExtensions
    {
        #region Deprecated Projection Registrations (No RetryPolicy specified)
        [Obsolete("Use overload with MessageHandlingRetryPolicy. Default policy: RetryPolicy.NoRetries", false)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(this ContainerBuilder builder)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => new TConnectedProjection(), RetryPolicy.NoRetries);

        [Obsolete("Use overload with MessageHandlingRetryPolicy. Default policy: RetryPolicy.NoRetries", false)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<TConnectedProjection> projectionFactory)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => projectionFactory(), RetryPolicy.NoRetries);

        [Obsolete("Use overload with MessageHandlingRetryPolicy. Default policy: RetryPolicy.NoRetries", false)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<IComponentContext, TConnectedProjection> projectionFactory)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(projectionFactory, RetryPolicy.NoRetries);
        #endregion

        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            MessageHandlingRetryPolicy retryPolicy)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => new TConnectedProjection(), retryPolicy);

        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<TConnectedProjection> projectionFactory,
            MessageHandlingRetryPolicy retryPolicy)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => projectionFactory(), retryPolicy);

        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<IComponentContext, TConnectedProjection> projectionFactory,
            MessageHandlingRetryPolicy retryPolicy)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
        {
            builder
                .Register(container =>
                    new ConnectedProjection<TConnectedProjection, TContext>(
                        container.Resolve<Func<Owned<TContext>>>(),
                        projectionFactory(container),
                        retryPolicy,
                        container.Resolve<EnvelopeFactory>(),
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
