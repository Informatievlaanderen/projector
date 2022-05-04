namespace Be.Vlaanderen.Basisregisters.Projector.Modules
{
    using Autofac;
    using Autofac.Core;
    using ConnectedProjections;
    using Internal;
    using Internal.Commands;
    using Internal.Configuration;
    using Internal.Runners;
    using Internal.StreamGapStrategies;
    using Microsoft.Extensions.Configuration;
    using NodaTime;
    using Module = Autofac.Module;

    public class ProjectorModule : Module
    {
        private readonly IConfiguration _configuration;

        public ProjectorModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(SystemClock.Instance)
                .As<IClock>();

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

            builder.RegisterGeneric(typeof(StreamStoreConnectedProjectionContext<>))
                .As(typeof(IConnectedProjectionContext<>));

            builder.RegisterType<ConnectedProjectionsStreamStoreSubscription>()
                .As<IConnectedProjectionsStreamStoreSubscription>()
                .SingleInstance();

            var streamGapStrategySettings = new StreamGapStrategyConfigurationSettings();
            _configuration.Bind("streamGapStrategy", streamGapStrategySettings);
            builder.RegisterInstance(streamGapStrategySettings)
                .As<IStreamGapStrategyConfigurationSettings>();

            builder.RegisterAssemblyTypes(typeof(IStreamGapStrategy).Assembly)
                .Where(type => type.IsAssignableTo<IStreamGapStrategy>())
                .AsSelf();

            builder.RegisterType<StreamStoreConnectedProjectionsSubscriptionRunner>()
                .WithParameter(ResolveStrategy<DefaultSubscriptionStreamGapStrategy>())
                .As<IConnectedProjectionsSubscriptionRunner>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsCatchUpRunner>()
                .WithParameter(ResolveStrategy<DefaultCatchUpStreamGapStrategy>())
                .As<IConnectedProjectionsCatchUpRunner>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsCommandHandler>()
                .As<IConnectedProjectionsCommandHandler>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsManager>()
                .As<IConnectedProjectionsManager>()
                .SingleInstance();
        }

        private static ResolvedParameter ResolveStrategy<TGapStrategy>()
            where TGapStrategy : IStreamGapStrategy
            => new ResolvedParameter(
                (info, _) => info.ParameterType == typeof(IStreamGapStrategy),
                (_, context) => context.Resolve<TGapStrategy>());
    }
}
