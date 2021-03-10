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
