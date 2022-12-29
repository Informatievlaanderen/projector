namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Modules
{
    using System.Linq;
    using ConnectedProjections;
    using DependencyInjection;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using Internal;
    using Internal.Commands;
    using Internal.Configuration;
    using Internal.Runners;
    using Internal.StreamGapStrategies;
    using NodaTime;
    using SqlStreamStore;

    public class ProjectorModule : IServiceCollectionModule
    {
        private readonly IConfiguration _configuration;

        public ProjectorModule(IConfiguration configuration)
            => _configuration = configuration;

        public void Load(IServiceCollection services)
        {
            services
                .AddTransient<IClock>(_ => SystemClock.Instance)
                .AddSingleton<IConnectedProjectionsCommandBusHandlerConfiguration, ConnectedProjectionsCommandBus>()
                .AddSingleton<IConnectedProjectionsCommandBus, ConnectedProjectionsCommandBus>()
                .AddSingleton<IMigrationHelper, MigrationHelper>()
                .AddSingleton<IRegisteredProjections, RegisteredProjections>()
                .AddTransient(typeof(IConnectedProjectionContext<>), typeof(ConnectedProjectionContext<>))
                .AddSingleton<IConnectedProjectionsStreamStoreSubscription, ConnectedProjectionsStreamStoreSubscription>();

            var streamGapStrategySettings = new StreamGapStrategyConfigurationSettings();
            _configuration.Bind("streamGapStrategy", streamGapStrategySettings);
            services.AddTransient<IStreamGapStrategyConfigurationSettings>(_ => streamGapStrategySettings);

            typeof(IStreamGapStrategy).Assembly
                .GetTypes()
                .Where(x => x.IsAssignableTo<IStreamGapStrategy>())
                .ToList()
                .ForEach(x => services.AddTransient(x));

            services.AddSingleton<IConnectedProjectionsSubscriptionRunner, ConnectedProjectionsSubscriptionRunner>(_ => new ConnectedProjectionsSubscriptionRunner(
                    _.GetRequiredService<IRegisteredProjections>(),
                    _.GetRequiredService<IConnectedProjectionsStreamStoreSubscription>(),
                    _.GetRequiredService<IConnectedProjectionsCommandBus>(),
                    _.GetRequiredService<DefaultSubscriptionStreamGapStrategy>(),
                    _.GetRequiredService<ILoggerFactory>()));

            services.AddSingleton<IConnectedProjectionsCatchUpRunner, ConnectedProjectionsCatchUpRunner>(_ => new ConnectedProjectionsCatchUpRunner(
                    _.GetRequiredService<IRegisteredProjections>(),
                    _.GetRequiredService<IReadonlyStreamStore>(),
                    _.GetRequiredService<IConnectedProjectionsCommandBus>(),
                    _.GetRequiredService<DefaultSubscriptionStreamGapStrategy>(),
                    _.GetRequiredService<ILoggerFactory>()));

            services.AddSingleton<IConnectedProjectionsCommandHandler, ConnectedProjectionsCommandHandler>();

            services.AddSingleton<IConnectedProjectionsManager, ConnectedProjectionsManager>();
        }
    }
}
