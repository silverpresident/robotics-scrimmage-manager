using System.Threading.Tasks;

namespace RoboticsManager.Lib.Hubs
{
    public interface IUpdateClient
    {
        Task ReceiveLeaderboardUpdate();
        Task ReceiveAnnouncement();
        Task ReceiveUpdate();
        Task ReceiveChallengeCompletion(string teamName, string challengeName, int points);
        Task ReceiveTeamUpdate(string teamName);
        Task ReceiveChallengeUpdate(string challengeName);
        Task ReceiveTeamSpecificUpdate(string message);
        Task ReceiveChallengeSpecificUpdate(string message);
        Task ReceiveAdminUpdate(string message);
        Task ReceiveJudgeUpdate(string message);
    }
}
