namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class ProjectionsManagerOptions
    {
        public CommonOptions Common { get; } = new CommonOptions();

        public class CommonOptions
        {
            public IServiceProvider ServiceProvider { get; set; }
            public IHostApplicationLifetime ApplicationLifetime { get; set; }
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

        public static IApplicationBuilder UseProjectionsManagerAsync(
            this IApplicationBuilder app,
            ProjectionsManagerOptions options)
        {
            options.Common.ApplicationLifetime.ApplicationStopping.Register(() => ProjectionsCancellationTokenSource.Cancel());

            Task.Run(() =>
            {
                var projectionsManager = options.Common.ServiceProvider.GetRequiredService<IConnectedProjectionsManager>();
                projectionsManager.Resume(ProjectionsCancellationTokenSource.Token);
            }, ProjectionsCancellationTokenSource.Token);

            return app;
        }
    }
}
