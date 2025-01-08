namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using SqlStreamStore;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseProjectorEndpoints(
            this IApplicationBuilder builder,
            string baseUrl,
            JsonSerializerSettings? jsonSerializerSettings = null)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);

            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/v1/projections", async context => { await GetProjections(builder, context, baseUrl, jsonSerializerSettings).NoContext(); });
                endpoints.MapGet("/projections", async context => { await GetProjections(builder, context, baseUrl, jsonSerializerSettings).NoContext(); });

                endpoints.MapPost("/projections/start/all", async context => { await StartAll(builder, context).NoContext(); });
                endpoints.MapPost("/v1/projections/start/all", async context => { await StartAll(builder, context).NoContext(); });

                endpoints.MapPost("/projections/start/{projectionId}", async context
                    => await StartProjection(builder, context.Request.RouteValues["projectionId"].ToString(), context).NoContext());
                endpoints.MapPost("/v1/projections/start/{projectionId}", async context
                    => await StartProjection(builder, context.Request.RouteValues["projectionId"].ToString(), context).NoContext());

                endpoints.MapPost("/projections/stop/all", async context => { await StopAll(builder, context).NoContext(); });
                endpoints.MapPost("/v1/projections/stop/all", async context => { await StopAll(builder, context).NoContext(); });

                endpoints.MapPost("/projections/stop/{projectionId}", async context
                    => await StopProjection(builder, context.Request.RouteValues["projectionId"].ToString(), context).NoContext());
                endpoints.MapPost("/v1/projections/stop/{projectionId}", async context
                    => await StopProjection(builder, context.Request.RouteValues["projectionId"].ToString(), context).NoContext());
            });

            return builder;
        }

        private static async Task StopProjection(IApplicationBuilder app, string? projectionId, HttpContext context)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            if (!manager.Exists(projectionId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid projection Id.").NoContext();
                return;
            }

            await manager.Stop(projectionId, CancellationToken.None).NoContext();
            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private static async Task StopAll(IApplicationBuilder app, HttpContext context)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            await manager.Stop(CancellationToken.None).NoContext();

            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private static async Task StartProjection(IApplicationBuilder app, string? projectionId, HttpContext context)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            if (!manager.Exists(projectionId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid projection Id.").NoContext();
                return;
            }

            await manager.Start(projectionId, CancellationToken.None).NoContext();
            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private static async Task StartAll(IApplicationBuilder app, HttpContext context)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            await manager.Start(CancellationToken.None).NoContext();

            context.Response.StatusCode = StatusCodes.Status202Accepted;
        }

        private static async Task GetProjections(
            IApplicationBuilder app,
            HttpContext context,
            string baseUrl,
            JsonSerializerSettings? jsonSerializerSettings = null)
        {
            var manager = app.ApplicationServices.GetRequiredService<IConnectedProjectionsManager>();
            var streamStore = app.ApplicationServices.GetRequiredService<IStreamStore>();

            var registeredConnectedProjections = manager
                .GetRegisteredProjections()
                .ToList();
            var projectionStates = await manager.GetProjectionStates(CancellationToken.None).NoContext();
            var responses = registeredConnectedProjections.Aggregate(
                new List<ProjectionResponse>(),
                (list, projection) =>
                {
                    var projectionState = projectionStates.SingleOrDefault(x => x.Name == projection.Id);
                    list.Add(new ProjectionResponse(
                        projection,
                        projectionState,
                        baseUrl));
                    return list;
                });

            var streamPosition = await streamStore.ReadHeadPosition().NoContext();

            var projectionResponseList = new ProjectionResponseList(responses, baseUrl)
            {
                StreamPosition = streamPosition
            };

            var json = JsonConvert.SerializeObject(projectionResponseList, jsonSerializerSettings ?? new JsonSerializerSettings());

            context.Response.Headers.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(json).NoContext();
        }
    }
}
