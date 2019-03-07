namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System;
    using Autofac;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Internal;
    using ProjectionHandling.Runner;

    public static class ContainerBuilderExtensions
    {
        public static void RegisterProjections<TConnectedProjection, TContext>(this ContainerBuilder builder)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
        {
            builder.RegisterProjections<TConnectedProjection,TContext>(Activator.CreateInstance<TConnectedProjection>);
        }

        public static void RegisterProjections<TConnectedProjection, TContext>(this ContainerBuilder builder, Func<TConnectedProjection> projectionFactory)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
        {
            builder
                .Register(container =>
                    new ConnectedProjectionRegistrationRegistration<TConnectedProjection, TContext>(
                        projectionFactory(),
                        container.Resolve<Func<Owned<TContext>>>())
                )
                .As<IConnectedProjectionRegistration>();
        }
    }
}
