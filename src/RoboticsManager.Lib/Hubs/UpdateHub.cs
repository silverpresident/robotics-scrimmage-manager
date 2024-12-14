using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace RoboticsManager.Lib.Hubs
{
    public class UpdateHub : Hub<IUpdateClient>
    {
        private readonly ILogger<UpdateHub> _logger;

        public UpdateHub(ILogger<UpdateHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Called when a team's points are updated
        public async Task NotifyLeaderboardUpdate()
        {
            await Clients.All.ReceiveLeaderboardUpdate();
        }

        // Called when a new announcement is created or updated
        public async Task NotifyAnnouncementUpdate()
        {
            await Clients.All.ReceiveAnnouncement();
        }

        // Called when a new update is created
        public async Task NotifyUpdate()
        {
            await Clients.All.ReceiveUpdate();
        }

        // Called when a challenge is completed
        public async Task NotifyChallengeCompletion(string teamName, string challengeName, int points)
        {
            await Clients.All.ReceiveChallengeCompletion(teamName, challengeName, points);
        }

        // Called when a team is added or updated
        public async Task NotifyTeamUpdate(string teamName)
        {
            await Clients.All.ReceiveTeamUpdate(teamName);
        }

        // Called when a challenge is added or updated
        public async Task NotifyChallengeUpdate(string challengeName)
        {
            await Clients.All.ReceiveChallengeUpdate(challengeName);
        }

        // Group management for specific updates
        public async Task JoinTeamGroup(string teamId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"team_{teamId}");
        }

        public async Task LeaveTeamGroup(string teamId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"team_{teamId}");
        }

        public async Task JoinChallengeGroup(string challengeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"challenge_{challengeId}");
        }

        public async Task LeaveChallengeGroup(string challengeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"challenge_{challengeId}");
        }

        // Send update to specific team's group
        public async Task SendTeamSpecificUpdate(string teamId, string message)
        {
            await Clients.Group($"team_{teamId}").ReceiveTeamSpecificUpdate(message);
        }

        // Send update to specific challenge's group
        public async Task SendChallengeSpecificUpdate(string challengeId, string message)
        {
            await Clients.Group($"challenge_{challengeId}").ReceiveChallengeSpecificUpdate(message);
        }

        // Send admin-only updates
        public async Task SendAdminUpdate(string message)
        {
            await Clients.Group("Administrators").ReceiveAdminUpdate(message);
        }

        // Send judge-only updates
        public async Task SendJudgeUpdate(string message)
        {
            await Clients.Group("Judges").ReceiveJudgeUpdate(message);
        }
    }
}
