using System;
using System.Threading.Tasks;
using UnityEngine;

public abstract class TransportMatchmaking : MonoBehaviour
{
    public event Action<bool> IsLookingForLobby;
    protected event Action<MatchmakingParameters> ONLobbyQuery;

    [SerializeField] protected Transport transport;
    [SerializeField] protected int startRankingRange = 30;
    [SerializeField] protected int searches = 50;
    [SerializeField] protected int standardRatingGain = 30;
    [SerializeField] protected int rangeIncreasePerEachSearch;

    protected int ranking;
    
    public int Ranking() => ranking;
    public int lobbyRanking;


    protected void InvokeIsLookingForLobby(bool isLooking) => IsLookingForLobby?.Invoke(isLooking);
    public void SetLobbyRanking(int lobby) => lobbyRanking = lobby;

    
    protected virtual void Awake()
    {
        ONLobbyQuery += OnPlayMatch;
    }
    protected virtual void OnDestroy()
    {
        ONLobbyQuery -= OnPlayMatch;
    }


    public void QueryLobby(MatchmakingParameters matchmakingParameters)
    {
        ONLobbyQuery?.Invoke(matchmakingParameters);
    }

    private void OnPlayMatch(MatchmakingParameters matchmakingParameters)
    {
        if (matchmakingParameters.isRanked)
        {
            matchmakingParameters.minPlayers = 2;
        }
        else
        {
            matchmakingParameters.minPlayers = 1;
        }
        
        PlayGame(matchmakingParameters);
    }

    protected abstract Task PlayGame(MatchmakingParameters parameters);

    
    public void SetRanking(bool victory) => SetPlayerRanking(victory);

    protected virtual void SetPlayerRanking(bool victory)
    {
        var ratingToChange = CalculateRanking(victory);
        ranking += ratingToChange;
        if (ranking < 0) ranking = 0;

        Debug.Log("Rating to change: " + ratingToChange + ", ranking after calculation: " + ranking);
    }
    
    private int CalculateRanking(bool victory)
    {
        var difference = ranking - lobbyRanking;
        Debug.Log("Difference in rating:" + difference + ". Player: " + ranking + "/lobby: " + lobbyRanking);
        int ratingToChange;
        if (victory)
        {
            ratingToChange = standardRatingGain + difference / 10;
        }
        else
        {
            ratingToChange = -((standardRatingGain + difference / 10) / 3);
        }

        if (ratingToChange > 50) ratingToChange = 50;
        if (ratingToChange < -50) ratingToChange = -50;
        return ratingToChange;
    }
    
    protected int CalculateRange(int range, out int higher)
    {
        var lower = ranking + range;
        higher = ranking - range;
        if (higher < 0) higher = -1;
        Debug.Log("Looking for lobby in range: " + higher + " / " + lower);
        return lower;
    }
    
    protected void StartHostingGame(MatchmakingParameters parameters)
    {
        GameNetPortalBase.Instance.SetMultiplayerLobbyScene(parameters);
        transport.StartTransportHost(parameters.isRanked);
    }
}