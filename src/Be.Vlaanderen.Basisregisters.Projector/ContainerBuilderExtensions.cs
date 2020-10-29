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

    public static class ContainerBuilderExtensions
    {
        #region Deprecated Projection Registrations (without projection settings)
        [Obsolete("Use overload with ConnectedProjectionSettings. Default policy: RetryPolicy.NoRetries", true)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(this ContainerBuilder builder)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => new TConnectedProjection(), ConnectedProjectionSettings.Default);

        [Obsolete("Use overload with ConnectedProjectionSettings. Default policy: RetryPolicy.NoRetries", true)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<TConnectedProjection> projectionFactory)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => projectionFactory(), ConnectedProjectionSettings.Default);

        [Obsolete("Use overload with ConnectedProjectionSettings. Default policy: RetryPolicy.NoRetries", true)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<IComponentContext, TConnectedProjection> projectionFactory)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(projectionFactory, ConnectedProjectionSettings.Default);

        [Obsolete("Use overload with ConnectedProjectionSettings", true)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            MessageHandlingRetryPolicy retryPolicy)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => new TConnectedProjection(), ConnectedProjectionSettings.Configure(configurator => configurator.SetPolicy(retryPolicy)));

        [Obsolete("Use overload with ConnectedProjectionSettings", true)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<TConnectedProjection> projectionFactory,
            MessageHandlingRetryPolicy retryPolicy)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => projectionFactory(), ConnectedProjectionSettings.Configure(configurator => configurator.SetPolicy(retryPolicy)));

        [Obsolete("Use overload with ConnectedProjectionSettings", true)]
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<IComponentContext, TConnectedProjection> projectionFactory,
            MessageHandlingRetryPolicy retryPolicy)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(projectionFactory, ConnectedProjectionSettings.Configure(configurator => configurator.SetPolicy(retryPolicy).CreateSettings()));
        #endregion
		
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            ConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => new TConnectedProjection(), settings);

        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<TConnectedProjection> projectionFactory,
            ConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => projectionFactory(), settings);

        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(
            this ContainerBuilder builder,
            Func<IComponentContext, TConnectedProjection> projectionFactory,
            ConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
        {
            builder
                .Register(container =>
                    new ConnectedProjection<TConnectedProjection, TContext>(
                        container.Resolve<Func<Owned<IConnectedProjectionContext<TContext>>>>(),
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
