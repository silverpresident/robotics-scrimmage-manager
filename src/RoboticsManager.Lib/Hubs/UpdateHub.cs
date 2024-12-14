using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Lib.Hubs
{
    public interface IUpdateClient
    {
        Task ReceiveUpdate(Update update);
        Task ReceiveTeamUpdate(Team team);
        Task ReceiveAnnouncement(Announcement announcement);
        Task ReceiveLeaderboardUpdate(Team[] leaderboard);
        Task ReceiveChallengeUpdate(Challenge challenge);
    }

    public class UpdateHub : Hub<IUpdateClient>
    {
        public const string HubUrl = "/hubs/updates";

        public async Task SendUpdate(Update update)
        {
            await Clients.All.ReceiveUpdate(update);
        }

        public async Task SendTeamUpdate(Team team)
        {
            await Clients.All.ReceiveTeamUpdate(team);
        }

        public async Task SendAnnouncement(Announcement announcement)
        {
            await Clients.All.ReceiveAnnouncement(announcement);
        }

        public async Task SendLeaderboardUpdate(Team[] leaderboard)
        {
            await Clients.All.ReceiveLeaderboardUpdate(leaderboard);
        }

        public async Task SendChallengeUpdate(Challenge challenge)
        {
            await Clients.All.ReceiveChallengeUpdate(challenge);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
