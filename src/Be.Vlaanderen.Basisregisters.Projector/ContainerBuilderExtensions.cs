namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System;
    using Autofac;
    using Autofac.Features.OwnedInstances;
    using Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;

    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(this ContainerBuilder builder)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => new TConnectedProjection());

        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(this ContainerBuilder builder, Func<TConnectedProjection> projectionFactory)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => builder.RegisterProjections<TConnectedProjection, TContext>(container => projectionFactory());

        public static ContainerBuilder RegisterProjections<TConnectedProjection, TContext>(this ContainerBuilder builder, Func<IComponentContext, TConnectedProjection> projectionFactory)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
        {
            builder
                .Register(container =>
                    new ConnectedProjection<TConnectedProjection, TContext>(
                        container.Resolve<Func<Owned<TContext>>>(),
                        projectionFactory(container),
                        container.Resolve<EnvelopeFactory>(),
                        container.Resolve<ILoggerFactory>()))
                .As<IConnectedProjection>();

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
    }
}
