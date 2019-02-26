namespace Be.Vlaanderen.Basisregisters.Projector.Modules
{
    using System;
    using System.Reflection;
    using Autofac;
    using ConnectedProjections;
    using Internal;
    using Internal.Runners;
    using Module = Autofac.Module;

    public class ProjectorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ConnectedProjectionProcessEventBus>()
                .As<IConnectedProjectionEventBus>()
                .As<IConnectedProjectionEventHandler>()
                .SingleInstance();

            builder
                .RegisterType<ConnectedProjectionsCatchUpRunner>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<ConnectedProjectionsSubscriptionRunner>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<ConnectedProjectionsManager>()
                .FindConstructorsWith(AllowNonPublicConstructor)
                .AsSelf()
                .SingleInstance();
        }

        private static ConstructorInfo[] AllowNonPublicConstructor(Type type)
        {
            return type.GetConstructors(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );
        }
    }
}
