namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft
{
    using System;
    using System.Linq;
    using DependencyInjection.OwnedInstances;
    using ConnectedProjections;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using Internal;
    using ProjectionHandling.Runner.Microsoft;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterProjections<TConnectedProjection, TContext>(
            this IServiceCollection services,
            ConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>, new()
            where TContext : RunnerDbContext<TContext>
            => services.RegisterProjections<TConnectedProjection, TContext>(_ => new TConnectedProjection(), settings);

        public static IServiceCollection RegisterProjections<TConnectedProjection, TContext>(
            this IServiceCollection services,
            Func<TConnectedProjection> projectionFactory,
            ConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
            => services.RegisterProjections<TConnectedProjection, TContext>(_ => projectionFactory(), settings);

        public static IServiceCollection RegisterProjections<TConnectedProjection, TContext>(
            this IServiceCollection services,
            Func<IServiceProvider, TConnectedProjection> projectionFactory,
            ConnectedProjectionSettings settings)
            where TConnectedProjection : ProjectionHandling.Connector.ConnectedProjection<TContext>
            where TContext : RunnerDbContext<TContext>
        {
            //if (ConcreteTypeIsNotRegistered<IConnectedProjection>())
            //{
                
            //}
            services.AddTransient<IConnectedProjection>(serviceProvider => new ConnectedProjection<TConnectedProjection, TContext>(
                    serviceProvider.GetRequiredService<Func<Owned<IConnectedProjectionContext<TContext>>>>(),
                    projectionFactory(serviceProvider),
                    settings,
                    serviceProvider.GetRequiredService<ILoggerFactory>()));
                //.IfConcreteTypeIsNotRegistered();

            return services;
        }

        public static IServiceCollection RegisterProjectionMigrator<TContextMigrationFactory>(this IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory)
            where TContextMigrationFactory : IRunnerDbContextMigratorFactory, new()
        {
            services.AddTransient(_ => new TContextMigrationFactory().CreateMigrator(configuration, loggerFactory));

            return services;
        }

        private static bool ConcreteTypeIsNotRegistered<T>(this IServiceCollection services) => services.All(x => x.ImplementationType != typeof(T));
    }
}
