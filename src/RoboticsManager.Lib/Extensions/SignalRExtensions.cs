using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RoboticsManager.Lib.Hubs;

namespace RoboticsManager.Lib.Extensions
{
    public static class SignalRExtensions
    {
        public static IServiceCollection AddRoboticsSignalR(this IServiceCollection services)
        {
            services.AddSignalR(options =>
            {
                // Configure SignalR options
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
                options.StreamBufferCapacity = 10;
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            });

            return services;
        }

        public static IApplicationBuilder UseRoboticsSignalR(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<UpdateHub>("/hubs/updates", options =>
                {
                    // Configure hub options
                    options.ApplicationMaxBufferSize = 64 * 1024; // 64KB
                    options.TransportMaxBufferSize = 64 * 1024;   // 64KB
                    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(5);
                    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
                });
            });

            return app;
        }
    }
}
