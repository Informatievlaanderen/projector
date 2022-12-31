namespace Be.Vlaanderen.Basisregisters.Projector.Modules
{
    using Autofac;
    using Autofac.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using SqlStreamStore;
    using System.Linq;
    using DependencyInjection;
    using IntAutofacCommands = Internal.Commands;
    using IntAutofacConfiguration = Internal.Configuration;
    using IntAutofacRunners = Internal.Runners;
    using IntAutofacStreamGapStrategies = Internal.StreamGapStrategies;
    using IntMicrosoftCommands = InternalMicrosoft.Commands;
    using IntMicrosoftConfiguration = InternalMicrosoft.Configuration;
    using IntMicrosoftRunners = InternalMicrosoft.Runners;
    using IntMicrosoftStreamGapStrategies = InternalMicrosoft.StreamGapStrategies;

    public class ProjectorModule : Module, IServiceCollectionModule
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

            builder.RegisterType<Internal.MigrationHelper>()
                .As<Internal.IMigrationHelper>()
                .SingleInstance();

            builder.RegisterType<Internal.RegisteredProjections>()
                .As<Internal.IRegisteredProjections>()
                .SingleInstance();

            builder.RegisterGeneric(typeof(Internal.ConnectedProjectionContext<>))
                .As(typeof(Internal.IConnectedProjectionContext<>));

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

            builder.RegisterType<Internal.ConnectedProjectionsManager>()
                .As<ConnectedProjections.IConnectedProjectionsManager>()
                .SingleInstance();
        }

        private static ResolvedParameter ResolveStrategy<TGapStrategy>()
            where TGapStrategy : IntAutofacStreamGapStrategies.IStreamGapStrategy
            => new ResolvedParameter(
                (info, _) => info.ParameterType == typeof(IntAutofacStreamGapStrategies.IStreamGapStrategy),
                (_, context) => context.Resolve<TGapStrategy>());

        public void Load(IServiceCollection services)
        {
            services
                .AddTransient<IClock>(_ => SystemClock.Instance)
                .AddSingleton<IntMicrosoftCommands.IConnectedProjectionsCommandBusHandlerConfiguration, IntMicrosoftCommands.ConnectedProjectionsCommandBus>()
                .AddSingleton<IntMicrosoftCommands.IConnectedProjectionsCommandBus, IntMicrosoftCommands.ConnectedProjectionsCommandBus>()
                .AddSingleton<InternalMicrosoft.IMigrationHelper, InternalMicrosoft.MigrationHelper>()
                .AddSingleton<InternalMicrosoft.IRegisteredProjections, InternalMicrosoft.RegisteredProjections>()
                .AddTransient(typeof(InternalMicrosoft.IConnectedProjectionContext<>), typeof(InternalMicrosoft.ConnectedProjectionContext<>))
                .AddSingleton<IntMicrosoftRunners.IConnectedProjectionsStreamStoreSubscription, IntMicrosoftRunners.ConnectedProjectionsStreamStoreSubscription>();

            var streamGapStrategySettings = new IntMicrosoftConfiguration.StreamGapStrategyConfigurationSettings();
            _configuration.Bind("streamGapStrategy", streamGapStrategySettings);
            services.AddTransient<IntMicrosoftConfiguration.IStreamGapStrategyConfigurationSettings>(_ => streamGapStrategySettings);

            typeof(IntMicrosoftStreamGapStrategies.IStreamGapStrategy).Assembly
                .GetTypes()
                .Where(x => x.IsAssignableTo<IntMicrosoftStreamGapStrategies.IStreamGapStrategy>())
                .ToList()
                .ForEach(x => services.AddTransient(x));

            services.AddSingleton<IntMicrosoftRunners.IConnectedProjectionsSubscriptionRunner, IntMicrosoftRunners.ConnectedProjectionsSubscriptionRunner>(_ => new IntMicrosoftRunners.ConnectedProjectionsSubscriptionRunner(
                    _.GetRequiredService<InternalMicrosoft.IRegisteredProjections>(),
                    _.GetRequiredService<IntMicrosoftRunners.IConnectedProjectionsStreamStoreSubscription>(),
                    _.GetRequiredService<IntMicrosoftCommands.IConnectedProjectionsCommandBus>(),
                    _.GetRequiredService<IntMicrosoftStreamGapStrategies.DefaultSubscriptionStreamGapStrategy>(),
                    _.GetRequiredService<ILoggerFactory>()));

            services.AddSingleton<IntMicrosoftRunners.IConnectedProjectionsCatchUpRunner, IntMicrosoftRunners.ConnectedProjectionsCatchUpRunner>(_ => new IntMicrosoftRunners.ConnectedProjectionsCatchUpRunner(
                    _.GetRequiredService<InternalMicrosoft.IRegisteredProjections>(),
                    _.GetRequiredService<IReadonlyStreamStore>(),
                    _.GetRequiredService<IntMicrosoftCommands.IConnectedProjectionsCommandBus>(),
                    _.GetRequiredService<IntMicrosoftStreamGapStrategies.DefaultSubscriptionStreamGapStrategy>(),
                    _.GetRequiredService<ILoggerFactory>()));

            services.AddSingleton<IntMicrosoftCommands.IConnectedProjectionsCommandHandler, IntMicrosoftCommands.ConnectedProjectionsCommandHandler>();

            services.AddSingleton<ConnectedProjectionsMicrosoft.IConnectedProjectionsManager, InternalMicrosoft.ConnectedProjectionsManager>();
        }
    }
}
