using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // Seed roles
            await SeedRolesAsync(roleManager);

            // Seed admin user
            await SeedAdminUserAsync(userManager);

            // Seed initial data
            await SeedInitialDataAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Administrator", "Judge", "Scorekeeper" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@stjago.edu.jm";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };

                var result = await userManager.CreateAsync(adminUser, "StJago2024!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }
        }

        private static async Task SeedInitialDataAsync(ApplicationDbContext context)
        {
            // Seed sample challenges if none exist
            if (!await context.Challenges.AnyAsync())
            {
                var challenges = new[]
                {
                    new Challenge
                    {
                        Name = "Line Following",
                        Description = "Navigate the robot through a complex line course with intersections and curves.",
                        Points = 100,
                        IsUnique = false
                    },
                    new Challenge
                    {
                        Name = "Maze Navigation",
                        Description = "Guide the robot through a maze using sensors and autonomous control.",
                        Points = 150,
                        IsUnique = false
                    },
                    new Challenge
                    {
                        Name = "Object Sorting",
                        Description = "Sort colored objects into designated zones using computer vision.",
                        Points = 200,
                        IsUnique = false
                    },
                    new Challenge
                    {
                        Name = "First to Cross",
                        Description = "Be the first team to successfully cross the advanced obstacle course.",
                        Points = 300,
                        IsUnique = true
                    }
                };

                context.Challenges.AddRange(challenges);
            }

            // Seed initial announcement
            if (!await context.Announcements.AnyAsync())
            {
                var announcement = new Announcement
                {
                    Body = "# Welcome to ST JAGO ROBOTICS SCRIMMAGE 2024!\n\n" +
                          "Get ready for an exciting day of robotics challenges and competition. " +
                          "Good luck to all participating teams!\n\n" +
                          "## Important Information\n" +
                          "- Please check in at the registration desk\n" +
                          "- Safety briefing starts at 9:00 AM\n" +
                          "- Practice rounds begin at 10:00 AM",
                    Priority = AnnouncementPriority.Primary,
                    IsVisible = true
                };

                context.Announcements.Add(announcement);
            }

            await context.SaveChangesAsync();
        }
    }
}
