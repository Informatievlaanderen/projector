namespace Be.Vlaanderen.Basisregisters.Projector.Modules
{
    using Autofac;
    using ConnectedProjections;

    public class ProjectorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // ToDo: see if this works with the internal constructor
            builder
                .RegisterType<ConnectedProjectionsManager>()
                .AsSelf()
                .SingleInstance();

            //            builder
//                .Register(container => new ConnectedProjectionsManager(
//                    container.Resolve<IEnumerable<IRunnerDbContextMigrationHelper>>(),
//                    container.Resolve<IEnumerable<IConnectedProjectionRegistration>>(),
//                    container.Resolve<IReadonlyStreamStore>(),
//                    container.Resolve<ILoggerFactory>(),
//                    container.Resolve<EnvelopeFactory>()
//                    ))
//                .AsSelf()
//                .SingleInstance();
        }
    }
}
