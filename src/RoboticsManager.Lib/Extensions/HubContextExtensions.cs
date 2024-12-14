using Microsoft.AspNetCore.SignalR;
using RoboticsManager.Lib.Hubs;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Extensions
{
    public static class HubContextExtensions
    {
        public static async Task NotifyTeamPointsUpdated(this IHubContext<UpdateHub> hubContext, Team team)
        {
            await hubContext.Clients.All.SendAsync("ReceiveLeaderboardUpdate");
            await hubContext.Clients.Group($"team_{team.Id}").SendAsync(
                "ReceiveTeamSpecificUpdate",
                $"Team points updated: {team.TotalPoints}"
            );
        }

        public static async Task NotifyChallengeCompleted(
            this IHubContext<UpdateHub> hubContext,
            Team team,
            Challenge challenge)
        {
            await hubContext.Clients.All.SendAsync(
                "ReceiveChallengeCompletion",
                team.Name,
                challenge.Name,
                challenge.Points
            );

            // Notify team's group
            await hubContext.Clients.Group($"team_{team.Id}").SendAsync(
                "ReceiveTeamSpecificUpdate",
                $"Completed challenge: {challenge.Name} (+{challenge.Points} points)"
            );

            // Notify challenge's group
            await hubContext.Clients.Group($"challenge_{challenge.Id}").SendAsync(
                "ReceiveChallengeSpecificUpdate",
                $"Team {team.Name} completed this challenge"
            );

            // Update leaderboard
            await hubContext.Clients.All.SendAsync("ReceiveLeaderboardUpdate");
        }

        public static async Task NotifyAnnouncementCreated(
            this IHubContext<UpdateHub> hubContext,
            Announcement announcement)
        {
            await hubContext.Clients.All.SendAsync("ReceiveAnnouncement");

            // Notify admins with additional details
            await hubContext.Clients.Group("Administrators").SendAsync(
                "ReceiveAdminUpdate",
                $"New announcement created: {announcement.Body.Substring(0, Math.Min(50, announcement.Body.Length))}..."
            );
        }

        public static async Task NotifyAnnouncementUpdated(
            this IHubContext<UpdateHub> hubContext,
            Announcement announcement)
        {
            await hubContext.Clients.All.SendAsync("ReceiveAnnouncement");

            // Notify admins with additional details
            await hubContext.Clients.Group("Administrators").SendAsync(
                "ReceiveAdminUpdate",
                $"Announcement updated: {announcement.Body.Substring(0, Math.Min(50, announcement.Body.Length))}..."
            );
        }

        public static async Task NotifyTeamCreated(
            this IHubContext<UpdateHub> hubContext,
            Team team)
        {
            await hubContext.Clients.All.SendAsync("ReceiveTeamUpdate", team.Name);
            await hubContext.Clients.All.SendAsync("ReceiveLeaderboardUpdate");

            // Notify admins and judges
            var message = $"New team registered: {team.Name} ({team.School})";
            await hubContext.Clients.Group("Administrators").SendAsync("ReceiveAdminUpdate", message);
            await hubContext.Clients.Group("Judges").SendAsync("ReceiveJudgeUpdate", message);
        }

        public static async Task NotifyTeamUpdated(
            this IHubContext<UpdateHub> hubContext,
            Team team)
        {
            await hubContext.Clients.All.SendAsync("ReceiveTeamUpdate", team.Name);
            await hubContext.Clients.All.SendAsync("ReceiveLeaderboardUpdate");

            // Notify team's group
            await hubContext.Clients.Group($"team_{team.Id}").SendAsync(
                "ReceiveTeamSpecificUpdate",
                "Team information has been updated"
            );
        }

        public static async Task NotifyChallengeCreated(
            this IHubContext<UpdateHub> hubContext,
            Challenge challenge)
        {
            await hubContext.Clients.All.SendAsync("ReceiveChallengeUpdate", challenge.Name);

            // Notify admins and judges
            var message = $"New challenge created: {challenge.Name} ({challenge.Points} points)";
            await hubContext.Clients.Group("Administrators").SendAsync("ReceiveAdminUpdate", message);
            await hubContext.Clients.Group("Judges").SendAsync("ReceiveJudgeUpdate", message);
        }

        public static async Task NotifyChallengeUpdated(
            this IHubContext<UpdateHub> hubContext,
            Challenge challenge)
        {
            await hubContext.Clients.All.SendAsync("ReceiveChallengeUpdate", challenge.Name);

            // Notify challenge's group
            await hubContext.Clients.Group($"challenge_{challenge.Id}").SendAsync(
                "ReceiveChallengeSpecificUpdate",
                "Challenge information has been updated"
            );
        }

        public static async Task NotifyError(
            this IHubContext<UpdateHub> hubContext,
            string error,
            bool adminOnly = true)
        {
            if (adminOnly)
            {
                await hubContext.Clients.Group("Administrators").SendAsync(
                    "ReceiveAdminUpdate",
                    $"Error: {error}"
                );
            }
            else
            {
                await hubContext.Clients.All.SendAsync("ReceiveError", error);
            }
        }
    }
}
