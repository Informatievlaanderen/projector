namespace Be.Vlaanderen.Basisregisters.Projector.Modules
{
    using Autofac;
    using ConnectedProjections;
    using Internal;
    using Internal.Commands;
    using Internal.Runners;
    using Module = Autofac.Module;

    public class ProjectorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConnectedProjectionsCommandBus>()
                .AsSelf()
                .As<IConnectedProjectionsCommandBus>()
                .SingleInstance();

            builder.RegisterType<MigrationHelper>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<RegisteredProjections>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsStreamStoreSubscription>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsSubscriptionRunner>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsCatchUpRunner>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsCommandHandler>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsManager>()
                .As<IConnectedProjectionsManager>()
                .SingleInstance();
        }
    }
}
