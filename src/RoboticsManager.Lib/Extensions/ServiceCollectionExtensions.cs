using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoboticsManager.Lib.Configuration;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Services;
using RoboticsManager.Lib.Services.Implementations;

namespace RoboticsManager.Lib.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRoboticsManagerCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure options
            services.ConfigureRoboticsManager(configuration);
            var options = configuration.GetRoboticsManagerOptions();

            // Add DbContext
            services.AddDbContext<ApplicationDbContext>((serviceProvider, dbOptions) =>
            {
                dbOptions.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: options.Database.MaxRetryCount,
                            maxRetryDelay: TimeSpan.FromSeconds(options.Database.MaxRetryDelay),
                            errorNumbersToAdd: null);
                        sqlOptions.MigrationsAssembly("RoboticsManager.Lib");
                        sqlOptions.CommandTimeout(options.Database.CommandTimeout);
                    });

                if (options.Database.EnableDetailedErrors)
                {
                    dbOptions.EnableDetailedErrors();
                }

                if (options.Database.EnableSensitiveDataLogging)
                {
                    dbOptions.EnableSensitiveDataLogging();
                }
            });

            // Add SignalR
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = options.SignalR.EnableDetailedErrors;
                hubOptions.MaximumReceiveMessageSize = options.SignalR.MaximumReceiveMessageSize;
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(options.SignalR.KeepAliveInterval);
                hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(options.SignalR.ClientTimeoutInterval);
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(options.SignalR.HandshakeTimeout);
            });

            // Register Services
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IChallengeService, ChallengeService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IUpdateService, UpdateService>();

            return services;
        }

        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // Seed initial data
            await DbInitializer.InitializeAsync(serviceProvider);
        }
    }
}
