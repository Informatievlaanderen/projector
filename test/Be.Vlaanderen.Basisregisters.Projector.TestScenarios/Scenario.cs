namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
    using Newtonsoft.Json;
    using ProjectionHandling.Runner;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TestProjections;
    using TestProjections.Messages;
    using TestProjections.Projections;
    using System.Collections.Generic;
    using Internal.Extensions;

    public abstract class Scenario : IDisposable
    {
        private readonly ILifetimeScope _containerScope;
        private readonly string _databaseName;
        private readonly StreamId _streamId;
        private List<IEvent> _pushedMessages;

        protected IFixture Fixture { get; }
        protected Random Random { get; }
        
        protected Scenario()
        {
            Fixture = new Fixture();
            Random = new Random(Fixture.Create<int>());

            _databaseName = Guid.NewGuid().ToString();
            _streamId = new StreamId(Guid.NewGuid().ToString());
            _pushedMessages = new List<IEvent>();
            _containerScope = CreateContainer().BeginLifetimeScope();

            ProjectionManager = _containerScope.Resolve<IConnectedProjectionsManager>();

            // ReSharper disable once VirtualMemberCallInConstructor
            Task.Run(Setup).GetAwaiter().GetResult();
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
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings()));

            builder.RegisterModule<EnvelopeModule>();

            builder
                .RegisterInstance(new ProjectionContext(CreateContextOptionsFor<ProjectionContext>()))
                .AsSelf()
                .SingleInstance();

            ContainerSetup(builder);
            return builder.Build();
        }

        protected T Resolve<T>() => _containerScope.Resolve<T>();

        protected virtual Task Setup() => Task.CompletedTask;
        protected virtual void ContainerSetup(ContainerBuilder builder) {}
        
        protected IConnectedProjectionsManager ProjectionManager { get; set; }

        protected DbContextOptions<TContext> CreateContextOptionsFor<TContext>()
            where TContext : RunnerDbContext<TContext>
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(_databaseName, builder => {})
                .EnableSensitiveDataLogging()
                .Options;
        }


        protected async Task PushToStream(IEvent message) => await PushToStream(new[] { message });

        protected async Task PushToStream(IEnumerable<IEvent> messages)
        {
            var messageList = messages?.ToReadOnlyList();
            if (messageList == null || messageList.Count == 0)
                return;

            _pushedMessages.AddRange(messageList);
            await Resolve<IStreamStore>()
                .AppendToStream(
                    _streamId,
                    ExpectedVersion.Any,
                    messageList
                        .Select(message => new NewStreamMessage(
                            messageId: Guid.NewGuid(),
                            type: GetEventName(message),
                            jsonData: JsonConvert.SerializeObject(message)))
                        .ToArray(),
                    CancellationToken.None);
        }

        protected IReadOnlyList<IEvent> PushedMessages => _pushedMessages;

        private string GetEventName(IEvent message) => Resolve<EventMapping>().GetEventName(message.GetType());

        public void Dispose() => _containerScope.Dispose();
    }
}
