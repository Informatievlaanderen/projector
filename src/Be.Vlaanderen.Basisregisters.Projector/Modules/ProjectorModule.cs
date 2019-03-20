namespace Be.Vlaanderen.Basisregisters.Projector.Modules
{
    using System;
    using System.Reflection;
    using Autofac;
    using ConnectedProjections;
    using Internal;
    using Microsoft.Extensions.Logging;
    using Module = Autofac.Module;

    public class ProjectorModule : Module
    {
        private readonly ILoggerFactory _loggerFactory;

        public ProjectorModule(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConnectedProjectionsManager>()
                .FindConstructorsWith(AllowNonPublicConstructor)
                .WithParameter(new Parameter<ILoggerFactory>(_loggerFactory))
                .As<IProjectionManager>()
                .As<IConnectedProjectionsManager>()
                .SingleInstance();
        }

        private static ConstructorInfo[] AllowNonPublicConstructor(Type type)
        {
            return type.GetConstructors(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
        }
        
        private class Parameter<T> : TypedParameter
        {
            public Parameter(T value)
                : base(typeof(T), value)
            { }
        }
    }
}
