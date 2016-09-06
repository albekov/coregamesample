using System.Threading.Tasks;
using Game.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Game
{
    public class Startup
    {
        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterGameServices(services);

            services.AddRouting();
            services.AddSignalR(options => options.Hubs.EnableDetailedErrors = true);
        }

        [UsedImplicitly]
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime lifetime,
            IConnectionManager hubManager,
            ConnectionHandler connectionHandler,
            MainGame game)
        {
            loggerFactory.AddConsole(LogLevel.Trace);
            loggerFactory.AddDebug();

            var isDevelopment = env.IsDevelopment();

            if (isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }

            var routeBuilder = new RouteBuilder(app);
            var script = $"var debug={isDevelopment.ToString().ToLowerInvariant()};";
            routeBuilder.MapGet("settings.js", c => c.Response.WriteAsync(script));
            app.UseRouter(routeBuilder.Build());

            app.UseFileServer();

            app.UseWebSockets();
            app.UseSignalR();

            var stop = lifetime.ApplicationStopping;

            Task.Factory.StartNew(() => game.StartGame(stop), TaskCreationOptions.LongRunning);
        }

        private static void RegisterGameServices(IServiceCollection services)
        {
            services.AddSingleton<PlayerManager>();
            services.AddSingleton<World>();
            services.AddSingleton<ConnectionHandler>();
            services.AddSingleton<PlayersHandler>();
            services.AddSingleton<MainGame>();
        }
    }
}