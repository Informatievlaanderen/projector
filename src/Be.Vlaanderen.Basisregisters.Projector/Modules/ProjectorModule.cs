namespace Be.Vlaanderen.Basisregisters.Projector.Modules
{
    using Autofac;
    using Autofac.Core;
    using ConnectedProjections;
    using Internal;
    using Internal.Commands;
    using Internal.Runners;
    using Internal.StreamGapStrategies;
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
                .As<IRegisteredProjections>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsStreamStoreSubscription>()
                .As<IConnectedProjectionsStreamStoreSubscription>()
                .SingleInstance();

            builder.RegisterAssemblyTypes(typeof(IStreamGapStrategy).Assembly)
                .Where(type => type.IsAssignableTo<IStreamGapStrategy>())
                .AsSelf();

            builder.RegisterType<ConnectedProjectionsSubscriptionRunner>()
                .WithParameter(SetStrategy<DefaultSubscriptionStreamGapStrategy>())
                .As<IConnectedProjectionsSubscriptionRunner>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsCatchUpRunner>()
                .WithParameter(SetStrategy<DefaultCatchUpStreamGapStrategy>())
                .As<IConnectedProjectionsCatchUpRunner>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsCommandHandler>()
                .As<IConnectedProjectionsCommandHandler>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsManager>()
                .As<IConnectedProjectionsManager>()
                .SingleInstance();
        }

        private static ResolvedParameter SetStrategy<TGapStrategy>()
            where TGapStrategy : IStreamGapStrategy
            => new ResolvedParameter(
                (info, _) => info.ParameterType == typeof(IStreamGapStrategy),
                (_, context) => context.Resolve<TGapStrategy>());
    }
}
