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
                .As<IConnectedProjectionsCommandBusHandlerConfiguration>()
                .As<IConnectedProjectionsCommandBus>()
                .SingleInstance();

            builder.RegisterType<MigrationHelper>()
                .As<IMigrationHelper>()
                .SingleInstance();

            builder.RegisterType<RegisteredProjections>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsStreamStoreSubscription>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsSubscriptionRunner>()
                .As<IConnectedProjectionsSubscriptionRunner>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsCatchUpRunner>()
                .As<IConnectedProjectionsCatchUpRunner>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsCommandHandler>()
                .As<IConnectedProjectionsCommandHandler>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsManager>()
                .As<IConnectedProjectionsManager>()
                .SingleInstance();
        }
    }
}
