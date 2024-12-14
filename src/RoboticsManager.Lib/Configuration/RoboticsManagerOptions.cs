using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RoboticsManager.Lib.Configuration
{
    public class RoboticsManagerOptions
    {
        public const string ConfigurationSection = "RoboticsManager";

        public CompetitionOptions Competition { get; set; } = new();
        public ThemeOptions Theme { get; set; } = new();
        public SignalROptions SignalR { get; set; } = new();
        public DatabaseOptions Database { get; set; } = new();
    }

    public class CompetitionOptions
    {
        public string Name { get; set; } = "ST JAGO ROBOTICS SCRIMMAGE 2024";
        public int RefreshInterval { get; set; } = 300; // 5 minutes in seconds
        public int LeaderboardSize { get; set; } = 10;
        public int RecentAnnouncementsCount { get; set; } = 5;
        public int RecentUpdatesCount { get; set; } = 10;
    }

    public class ThemeOptions
    {
        public string PrimaryColor { get; set; } = "#000000"; // Black
        public string SecondaryColor { get; set; } = "#FFD700"; // Gold
        public string FontFamily { get; set; } = "system-ui, -apple-system, sans-serif";
        public string LogoUrl { get; set; } = "/images/logo.png";
    }

    public class SignalROptions
    {
        public string HubUrl { get; set; } = "/hubs/updates";
        public int MaximumReceiveMessageSize { get; set; } = 102400; // 100 KB
        public bool EnableDetailedErrors { get; set; } = false;
        public int KeepAliveInterval { get; set; } = 15; // seconds
        public int ClientTimeoutInterval { get; set; } = 30; // seconds
        public int HandshakeTimeout { get; set; } = 15; // seconds
    }

    public class DatabaseOptions
    {
        public int CommandTimeout { get; set; } = 30; // seconds
        public int MaxRetryCount { get; set; } = 5;
        public int MaxRetryDelay { get; set; } = 30; // seconds
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public bool EnableDetailedErrors { get; set; } = false;
    }

    public static class RoboticsManagerOptionsExtensions
    {
        public static IServiceCollection ConfigureRoboticsManager(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RoboticsManagerOptions>(
                configuration.GetSection(RoboticsManagerOptions.ConfigurationSection));

            return services;
        }

        public static RoboticsManagerOptions GetRoboticsManagerOptions(
            this IConfiguration configuration)
        {
            var options = new RoboticsManagerOptions();
            configuration.GetSection(RoboticsManagerOptions.ConfigurationSection).Bind(options);
            return options;
        }
    }
}
