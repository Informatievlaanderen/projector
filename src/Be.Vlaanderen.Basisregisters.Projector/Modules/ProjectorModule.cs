namespace Be.Vlaanderen.Basisregisters.Projector.Modules
{
    using Autofac;
    using Autofac.Core;
    using ConnectedProjections;
    using Internal;
    using Microsoft.Extensions.Configuration;
    using NodaTime;
    using IntAutofacCommands = Internal.Commands;
    using IntAutofacConfiguration = Internal.Configuration;
    using IntAutofacRunners = Internal.Runners;
    using IntAutofacStreamGapStrategies = Internal.StreamGapStrategies;

    public class ProjectorModule : Module
    {
        private readonly IConfiguration _configuration;

        public ProjectorModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(SystemClock.Instance)
                .As<IClock>();

            builder.RegisterType<IntAutofacCommands.ConnectedProjectionsCommandBus>()
                .As<IntAutofacCommands.IConnectedProjectionsCommandBusHandlerConfiguration>()
                .As<IntAutofacCommands.IConnectedProjectionsCommandBus>()
                .SingleInstance();

            builder.RegisterType<MigrationHelper>()
                .As<IMigrationHelper>()
                .SingleInstance();

            builder.RegisterType<RegisteredProjections>()
                .As<IRegisteredProjections>()
                .SingleInstance();

            builder.RegisterGeneric(typeof(ConnectedProjectionContext<>))
                .As(typeof(IConnectedProjectionContext<>));

            builder.RegisterType<IntAutofacRunners.ConnectedProjectionsStreamStoreSubscription>()
                .As<IntAutofacRunners.IConnectedProjectionsStreamStoreSubscription>()
                .SingleInstance();

            var streamGapStrategySettings = new IntAutofacConfiguration.StreamGapStrategyConfigurationSettings();
            _configuration.Bind("streamGapStrategy", streamGapStrategySettings);
            builder.RegisterInstance(streamGapStrategySettings)
                .As<IntAutofacConfiguration.IStreamGapStrategyConfigurationSettings>();

            builder.RegisterAssemblyTypes(typeof(IntAutofacStreamGapStrategies.IStreamGapStrategy).Assembly)
                .Where(type => type.IsAssignableTo<IntAutofacStreamGapStrategies.IStreamGapStrategy>())
                .AsSelf();

            builder.RegisterType<IntAutofacRunners.ConnectedProjectionsSubscriptionRunner>()
                .WithParameter(ResolveStrategy<IntAutofacStreamGapStrategies.DefaultSubscriptionStreamGapStrategy>())
                .As<IntAutofacRunners.IConnectedProjectionsSubscriptionRunner>()
                .SingleInstance();

            builder.RegisterType<IntAutofacRunners.ConnectedProjectionsCatchUpRunner>()
                .WithParameter(ResolveStrategy<IntAutofacStreamGapStrategies.DefaultCatchUpStreamGapStrategy>())
                .As<IntAutofacRunners.IConnectedProjectionsCatchUpRunner>()
                .SingleInstance();

            builder.RegisterType<IntAutofacCommands.ConnectedProjectionsCommandHandler>()
                .As<IntAutofacCommands.IConnectedProjectionsCommandHandler>()
                .SingleInstance();

            builder.RegisterType<ConnectedProjectionsManager>()
                .As<IConnectedProjectionsManager>()
                .SingleInstance();
        }

        private static ResolvedParameter ResolveStrategy<TGapStrategy>()
            where TGapStrategy : IntAutofacStreamGapStrategies.IStreamGapStrategy
            => new ResolvedParameter(
                (info, _) => info.ParameterType == typeof(IntAutofacStreamGapStrategies.IStreamGapStrategy),
                (_, context) => context.Resolve<TGapStrategy>());
    }
}
