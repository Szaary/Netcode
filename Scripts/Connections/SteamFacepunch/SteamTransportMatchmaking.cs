using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamTransportMatchmaking : TransportMatchmaking
{
    [SerializeField] private SteamFacade facade;
  

    protected override void Awake()
    {
        SteamUserStats.OnUserStatsReceived += OnUserStatsReceived;
        base.Awake();
    }

    protected override void OnDestroy()
    {
        SteamUserStats.OnUserStatsReceived -= OnUserStatsReceived;
        base.OnDestroy();
    }
    
    protected override async Task PlayGame(MatchmakingParameters parameters)
    {
        InvokeIsLookingForLobby(true);
        
        Lobby[] lobbies;
        try
        {
            if (parameters.isRanked)
                lobbies = await LookForRankedLobby(parameters);
            else
                lobbies = await LookForLobby(parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        if (lobbies == null)
        {
            StartHostingGame(parameters);
        }
        else
        {
            facade.client.AcceptLobbyInvitation((lobbies[0].Id).ToString());
        }
        InvokeIsLookingForLobby(false);
    }

    private async Task<Lobby[]> LookForRankedLobby(MatchmakingParameters matchmakingParameters)
    {
        var range = startRankingRange;

        for (var i = 0; i < searches; i++)
        {
            var lower = CalculateRange(range, out var higher);

            var query = SteamMatchmaking.LobbyList
                .WithKeyValue(MatchmakingKeys.GameModeKey, "true")
                .WithLower(MatchmakingKeys.RankingKey, lower)
                .WithHigher(MatchmakingKeys.RankingKey, higher)
                .FilterDistanceClose();
            try
            {
                var lobbies = await query.RequestAsync();
                if (lobbies.Length>0)
                {
                    Debug.Log("Found lobby");
                    return lobbies;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            range += rangeIncreasePerEachSearch;
        }
        return null;
    }

    private async Task<Lobby[]> LookForLobby(MatchmakingParameters matchmakingParameters)
    {
        for (var i = 0; i < searches; i++)
        {
            LobbyQuery query;
            if (matchmakingParameters.mapName == "")
            {
                query = SteamMatchmaking.LobbyList
                    .WithKeyValue(MatchmakingKeys.GameModeKey, "false")
                    .FilterDistanceClose();
            }
            else
            {
                query = SteamMatchmaking.LobbyList
                    .WithKeyValue(MatchmakingKeys.GameModeKey, "false")
                    .WithKeyValue(MatchmakingKeys.MapNameKey, matchmakingParameters.mapName)
                    .FilterDistanceClose();   
            }

            try
            {
                var lobbies = await query.RequestAsync();
                if (lobbies.Length>0)
                {
                    Debug.Log("Found lobby");
                    return lobbies;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        return null;
    }
    
    
    private void OnUserStatsReceived(SteamId arg1, Result arg2)
    {
        ranking = SteamUserStats.GetStatInt(MatchmakingKeys.RankingKey);
        if (ranking < 0)
        {
            ranking = 0;
            SteamUserStats.SetStat(MatchmakingKeys.RankingKey, ranking);
            facade.leaderboard.SetLeaderboardScore(ranking);
            facade.stats.Store();
        }
    }

    protected override void SetPlayerRanking(bool victory)
    {
        base.SetPlayerRanking(victory);

        SteamUserStats.SetStat(MatchmakingKeys.RankingKey, ranking);
        facade.leaderboard.SetLeaderboardScore(ranking);
        facade.stats.Store();
    }

  
}