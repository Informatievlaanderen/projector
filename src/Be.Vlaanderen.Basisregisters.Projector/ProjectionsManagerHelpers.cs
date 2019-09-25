namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System;
    using System.Threading;
    using ConnectedProjections;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public class ProjectionsManagerOptions
    {
        public CommonOptions Common { get; } = new CommonOptions();

        public class CommonOptions
        {
            public IServiceProvider ServiceProvider { get; set; }
            public IApplicationLifetime ApplicationLifetime { get; set; }
        }
    }

    public static class ProjectionsManagerExtensions
    {
        private static readonly CancellationTokenSource ProjectionsCancellationTokenSource = new CancellationTokenSource();

        public static IApplicationBuilder UseProjectionsManager(
            this IApplicationBuilder app,
            ProjectionsManagerOptions options)
        {
            options.Common.ApplicationLifetime.ApplicationStopping.Register(() => ProjectionsCancellationTokenSource.Cancel());

            var projectionsManager = options.Common.ServiceProvider.GetRequiredService<IConnectedProjectionsManager>();
            projectionsManager.Resume(ProjectionsCancellationTokenSource.Token);

            return app;
        }
    }
}
