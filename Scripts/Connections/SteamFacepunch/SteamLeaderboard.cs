using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamLeaderboard : TransportLeaderboard
{
    private Leaderboard leaderboard;

    private void Start()
    {
        GetLeaderboard();
    }

    private async void GetLeaderboard()
    {
        worldScores.Clear();
        friendsScores.Clear();

        if(!SteamClient.IsValid) return;
        
        var lb = await SteamUserStats.FindLeaderboardAsync(MatchmakingKeys.LeaderboardKey);
        if (lb.HasValue) leaderboard = lb.Value;
        else return;

        await GetScores();
    }

    private async Task GetScores()
    {
        var globalScores = await leaderboard.GetScoresAsync(10);
        if (globalScores==null || globalScores.Length == 0)
        {
            Debug.LogWarning("Leaderboard is empty");
            return;
        }

        foreach (var e in globalScores)
        {
            worldScores.Add(new PlayerScore() {name = e.User.Name, rank = e.GlobalRank, score = e.Score});
        }

        var friendScores = await leaderboard.GetScoresFromFriendsAsync();

        foreach (var e in friendScores)
        {
            friendsScores.Add(new PlayerScore() {name = e.User.Name, rank = e.GlobalRank, score = e.Score});
            if (e.User.IsMe)
            {
                playerScore = new PlayerScore() {name = e.User.Name, rank = e.GlobalRank, score = e.Score};
            }
        }


        worldScores = worldScores.OrderBy(x => x.rank).ToList();
        friendsScores = friendsScores.OrderBy(x => x.rank).ToList();
    }

    public override async void SetLeaderboardScore(int score)
    {
        var result = await leaderboard.ReplaceScore(score);
        if (result.HasValue)
        {
            Debug.Log("Player score after change: " + result.Value.Score);
        }

        await GetScores();
    }
}