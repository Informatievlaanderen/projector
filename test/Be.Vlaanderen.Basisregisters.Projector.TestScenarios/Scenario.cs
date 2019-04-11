namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using ConnectedProjections;
    using Modules;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using EventHandling;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using TestProjections;
    using TestProjections.Projections;

    public abstract class Scenario : IDisposable
    {
        private readonly ILifetimeScope _containerScope;
        private readonly string _databaseName;

        protected IFixture Fixture { get; }
        
        protected Scenario()
        {
            Fixture = new Fixture();
            _databaseName = Guid.NewGuid().ToString();

            _containerScope = CreateContainer().BeginLifetimeScope();

            ProjectionManager = _containerScope.Resolve<IConnectedProjectionsManager>();

            // ReSharper disable once VirtualMemberCallInConstructor
            Setup();
        }


        private IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<ProjectorModule>();
            builder.RegisterModule(new SqlStreamStoreModule(string.Empty, Schemas.Messages));
            builder
                .RegisterType<LoggerFactory>()
                .As<ILoggerFactory>()
                .SingleInstance();

            builder.RegisterModule(
                new EventHandlingModule(
                    typeof(DomainAssemblyMarker).Assembly,
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings()
                )
            );

            builder.RegisterModule<EnvelopeModule>();

            builder
                .RegisterInstance(new ProjectionContext(CreateContextOptionsFor<ProjectionContext>()))
                .AsSelf()
                .SingleInstance();

            ContainerSetup(builder);
            return builder.Build();
        }

        protected T Resolve<T>() => _containerScope.Resolve<T>();

        protected abstract void Setup();
        protected abstract void ContainerSetup(ContainerBuilder builder);
        
        protected IConnectedProjectionsManager ProjectionManager { get; set; }

        protected DbContextOptions<TContext> CreateContextOptionsFor<TContext>()
            where TContext : RunnerDbContext<TContext>
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(_databaseName, builder => {})
                .EnableSensitiveDataLogging()
                .Options;
        }

        public void Dispose()
        {
            _containerScope.Dispose();
        }
    }
}
