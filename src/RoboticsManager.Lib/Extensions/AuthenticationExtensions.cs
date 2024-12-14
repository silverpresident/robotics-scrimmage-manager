using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoboticsManager.Lib.Data;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddRoboticsAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure Entity Framework Identity
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("RoboticsManager.Web")
                )
            );

            // Add Identity services
            //services.AddDefaultIdentity<ApplicationUser>();
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            // Configure authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(12);
            })
            .AddGoogle(options =>
            {
                var googleAuth = configuration.GetSection("Authentication:Google");
                options.ClientId = googleAuth["ClientId"] ?? 
                    throw new InvalidOperationException("Google ClientId not configured");
                options.ClientSecret = googleAuth["ClientSecret"] ?? 
                    throw new InvalidOperationException("Google ClientSecret not configured");
                
                options.SaveTokens = true;
                options.Events.OnCreatingTicket = context =>
                {
                    // Extract additional claims from Google profile if needed
                    var email = context.Principal?.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                    {
                        // Add role claims based on configuration
                        var adminEmail = configuration["RoboticsManager:AdminEmail"];
                        var judgeEmails = configuration.GetSection("RoboticsManager:DefaultJudgeEmails").Get<string[]>();
                        var scorekeeperEmails = configuration.GetSection("RoboticsManager:DefaultScorekeeperEmails").Get<string[]>();

                        if (email == adminEmail)
                        {
                            context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                System.Security.Claims.ClaimTypes.Role, "Administrator"));
                        }
                        else if (judgeEmails?.Contains(email) == true)
                        {
                            context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                System.Security.Claims.ClaimTypes.Role, "Judge"));
                        }
                        else if (scorekeeperEmails?.Contains(email) == true)
                        {
                            context.Identity?.AddClaim(new System.Security.Claims.Claim(
                                System.Security.Claims.ClaimTypes.Role, "Scorekeeper"));
                        }
                    }

                    return Task.CompletedTask;
                };
            });

            // Configure authorization <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="7.0.0" />
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministrator", policy =>
                    policy.RequireRole("Administrator"));

                options.AddPolicy("RequireJudge", policy =>
                    policy.RequireRole("Administrator", "Judge"));

                options.AddPolicy("RequireScorekeeper", policy =>
                    policy.RequireRole("Administrator", "Judge", "Scorekeeper"));
            });

            return services;
        }
    }
}
