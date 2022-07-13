using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

public class SteamLobbyClient : TransportClient
{
    [SerializeField] private LogLevel logLevel;
    [SerializeField] private FacepunchTransport facepunchTransport;
    [SerializeField] private SteamFacade steamFacade;

    public ulong myLobbyId;
    private Lobby enteredLobby;


    public override void DeclineInvitation(string lobbyId)
    {
        Debug.Log("Not implemented");
    }

    public override void ClearLastSession()
    {
        base.ClearLastSession();
        enteredLobby = new Lobby();
    }


    public void Start()
    {
        if (!SteamClient.IsValid) return;

        SteamMatchmaking.OnLobbyInvite += LobbyInvited;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamMatchmaking.OnLobbyMemberDisconnected += LobbyDisconnected;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyMemberLeave += LobbyLeaved;
    }

    #region Joining last session by session id


    public override async Task<List<TransportLobby>> GetCreatedLobbiesBySession()
    {
        var lobbies = new List<TransportLobby>();

        var steamLobbies = await GetLastSessionLobby();

        if (steamLobbies.Count>0)
        {
            return lobbies;
        }

        foreach (var lobby in steamLobbies)
        {
            ulong.TryParse(lobby.GetData(MatchmakingKeys.HostKey), out var host);
            lobbies.Add(new TransportLobby()
            {
                lobbyId = lobby.Id.ToString(),
                hostId = host.ToString()
            });
        }

        return lobbies;
    }


    private async Task<List<Lobby>> GetLastSessionLobby()
    {
        List<Lobby> lobbies = new List<Lobby>();

        var lobbiesWithPlayerId = await GetLobbiesBySessionId();
        if (lobbiesWithPlayerId.Length>0)
        {
            return lobbies;
        }

        foreach (var lobby in lobbiesWithPlayerId)
        {
            lobby.Refresh();
        }

        var lobbiesWithPlayers = lobbiesWithPlayerId.Where(
            x => x.MemberCount != 0);

        lobbies.AddRange(lobbiesWithPlayers);
        return lobbies;
    }

    private async Task<Lobby[]> GetLobbiesBySessionId()
    {
        var query = SteamMatchmaking.LobbyList.WithKeyValue(MatchmakingKeys.SessionKey, sessionId);
        Lobby[] lobbies = await query.RequestAsync();
        return lobbies;
    }

    #endregion


    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    private void LobbyLeaved(Lobby arg1, Friend arg2)
    {
        MyLogger.Log("Lobby number: " + arg1.Id, logLevel);

        MyLogger.Log("Client leave with id: " + arg2.Id, logLevel);
        MyLogger.Log("Client status: " + arg2.State, logLevel);
        MyLogger.Log("Client is in game: " + arg2.IsPlayingThisGame, logLevel);
    }

    private void LobbyDisconnected(Lobby arg1, Friend arg2)
    {
        MyLogger.Log("Lobby number: " + arg1, logLevel);
        MyLogger.Log("Client disconnected with id: " + arg2.Id, logLevel);
        MyLogger.Log("Client status: " + arg2.State, logLevel);
        MyLogger.Log("Client is in game: " + arg2.IsPlayingThisGame, logLevel);
    }

    private void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        LobbyInvited(lobby.Owner, lobby);
    }

    private void LobbyInvited(Friend host, Lobby lobby)
    {
        MyLogger.Log("Invited to lobby id: " + lobby.Id, logLevel);

        if (CheckNetworkStatus()) return;

        ShowLobbyInvitation(host, lobby);
    }

    private bool CheckNetworkStatus()
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            MyLogger.Log("Client is already connected.", logLevel);
            return true;
        }

        if (NetworkManager.Singleton.ShutdownInProgress)
        {
            MyLogger.Log("Shutdown process in progress.", logLevel);
            return true;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            MyLogger.Log("Shutdown process in progress.", logLevel);
            return true;
        }
        return false;
    }

    private void ShowLobbyInvitation(Friend host, Lobby lobby)
    {
        if (FriendInvite.Instance != null)
        {
            FriendInvite.Instance.ShowInvitation(host.Name, lobby.Id.ToString());
        }
    }

    public override void AcceptLobbyInvitation(string id) =>
        SteamMatchmaking.JoinLobbyAsync(ulong.Parse(id));


    private void LobbyEntered(Lobby lobby)
    {
        SavingLobbyData(lobby);

        if (LockHostFromJoiningAsClient()) return;

        SetTransportData(lobby);

        StartTransportClient();
    }

    private void SavingLobbyData(Lobby lobby)
    {
        enteredLobby = lobby;
        myLobbyId = lobby.Id;
        MyLogger.Log("Lobby id: " + myLobbyId, logLevel);

        var data = lobby.GetData(MatchmakingKeys.SessionKey);
        MyLogger.Log("Got lobby data: " + data, logLevel);
        sessionId = data;
        steamFacade.host.sessionId = sessionId;

        var ranked = lobby.GetData(MatchmakingKeys.GameModeKey);
        if (ranked == "true")
        {
            Transport.Instance.isRankedGame = true;
        }
        else
        {
            Transport.Instance.isRankedGame = false;
        }
    }

    private bool LockHostFromJoiningAsClient()
    {
        MyLogger.Log("Saved session id: " + sessionId, logLevel);
        if (NetworkManager.Singleton.IsHost)
        {
            MyLogger.Log("Returning early. Host should not connect as client.", logLevel);
            return true;
        }

        return false;
    }

    private void SetTransportData(Lobby lobby)
    {
        var host = lobby.Members.First().Id;
        MyLogger.Log("Get Host id: " + host, logLevel);
        facepunchTransport.targetSteamId = host;
        MyLogger.Log("Connecting to host transport id: " + facepunchTransport.targetSteamId + "server id: " +
                     facepunchTransport.ServerClientId, logLevel);
    }

    private void StartTransportClient()
    {
        Id.ClearIdData();
  
        GameNetPortalBase.Instance.ConnectClient();
        LeaveLobby();
    }

    public override void LeaveLobby()
    {
        if (enteredLobby.Id.IsValid)
        {
            if (enteredLobby.IsOwnedBy(SteamClient.SteamId))
            {
                MyLogger.Log("Deleting data for reconnect to lobby:" + enteredLobby.Id, logLevel);
                enteredLobby.SetData(MatchmakingKeys.HostKey, "");
                enteredLobby.DeleteData(MatchmakingKeys.HostKey);
            }

            MyLogger.Log("Leaving Steam lobby:" + enteredLobby.Id, logLevel);
            enteredLobby.Leave();
        }
    }

    public override void JoinToNewHostGame(string newHost)
    {
        facepunchTransport.targetSteamId = ulong.Parse(newHost);
        GameNetPortalBase.Instance.ConnectClient();
    }
}