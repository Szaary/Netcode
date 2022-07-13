using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;


public class SteamLobbyHost : TransportHost
{
    [SerializeField] private LogLevel logLevel;
    [SerializeField] private SteamFacade facade;
    private Lobby currentLobby;
    List<Friend> activeFriends = new List<Friend>();
    private List<Lobby> reconnectLobbies = new List<Lobby>();


    public override void ClearLastSession()
    {
        base.ClearLastSession();
        currentLobby = new Lobby();
    }

    public void Start()
    {
        if (!SteamClient.IsValid) return;

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += LobbyMemberJoined;
        SteamFriends.OnPersonaStateChange += SteamFriendStatusChanged;
        activeFriends = SetActiveFriends();
    }

    private void SteamFriendStatusChanged(Friend obj)
    {
        activeFriends = SetActiveFriends();
    }

    public override List<TransportFriend> GetActiveFriends()
    {
        List<TransportFriend> active = new List<TransportFriend>();
        foreach (var friend in activeFriends)
        {
            var f = new TransportFriend
            {
                friendId = friend.Id.ToString(),
                friendName = friend.Name
            };
            active.Add(f);
        }

        return active;
    }

    private void LobbyMemberJoined(Lobby lobby, Friend steamId)
    {
        MyLogger.Log("Joined:" + steamId);
    }

    public override void LeaveLobbyAfterReconnect(string playerId)
    {
        Debug.Log("Steam lobby leave: " + playerId);
        foreach (var lobby in reconnectLobbies)
        {
            var reconnectedId = lobby.GetData(MatchmakingKeys.ReconnectedIdKey);

            Debug.Log("Got to lobbies: " + reconnectLobbies.Count + "reconnected player id: " + reconnectedId);

            if (reconnectedId == playerId)
            {
                Debug.Log("Leave lobby with reconnected player id: " + playerId);
                lobby.Leave();
                connectedIds.Remove(playerId);
            }
        }
    }

    public override void SetPrivateLobby(bool isActive)
    {
        if (Transport.Instance.isRankedGame)
        {
            currentLobby.SetPublic();
            isPrivateLobby = false;
            Debug.Log("In ranked game default lobby visibility is private");
            return;
        }

        if (isActive)
        {
            Debug.Log("Set private lobby");
            currentLobby.SetPrivate();
        }
        else
        {
            Debug.Log("Set public lobby");
            currentLobby.SetPublic();
        }

        isPrivateLobby = isActive;
    }


    public override void StartTransportHost()
    {
        base.StartTransportHost();
        if (!SteamClient.IsValid) return;
        SteamMatchmaking.CreateLobbyAsync(GameNetPortalBase.Instance.maxPlayers);
    }

    public override void StartTransportHostLastSession()
    {
        SteamMatchmaking.CreateLobbyAsync(GameNetPortalBase.Instance.maxPlayers);
    }

    public override void StartTransportHostLastSession(string clientId)
    {
        base.StartTransportHostLastSession(clientId);
        SteamMatchmaking.CreateLobbyAsync(GameNetPortalBase.Instance.maxPlayers);
    }

    private void OnLobbyCreated(Result callback, Lobby lobby)
    {
        currentLobby = lobby;

        SetHostData(lobby);
        SetRanking(lobby);
        SetSession(lobby);
        SetMap(lobby);
        SetPrivateLobby(isPrivateLobby);
        SetVisibility(lobby);

        lobby.SetJoinable(true);

        if (callback != Result.OK)
        {
            MyLogger.Log("Lobby Not Created", logLevel);
            MyLogger.Log("Reason: " + callback, logLevel);
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("New lobby created, but game exist, do not started sever");
                reconnectLobbies.Add(lobby);
                if (connectedIds.Count > 0)
                {
                    lobby.SetData(MatchmakingKeys.ReconnectedIdKey, connectedIds[0]);
                }

                return;
            }

            GameNetPortalBase.Instance.StartTransportHost();
        }
    }

    private void SetVisibility(Lobby lobby)
    {
        if (Transport.Instance.isRankedGame)
        {
            lobby.SetInvisible();
            Debug.Log("Set lobby invisible");
        }
        else
        {
            Debug.Log("Set lobby visible");
        }
    }

    private void SetMap(Lobby lobby)
    {
        var mapName = GameNetPortalBase.Instance.GetCurrentMap();
        lobby.SetData(MatchmakingKeys.MapNameKey, mapName);
        Debug.Log("Set map name: " + mapName);
    }

    private void SetSession(Lobby lobby)
    {
        if (sessionId=="")
        {
            sessionId = lobby.Id.ToString();
            reconnectLobbies.Clear();
            connectedIds.Clear();
            Debug.Log("This is new game, set session id to: " + sessionId);
        }

        lobby.SetData(MatchmakingKeys.SessionKey, sessionId);
        if (sessionId != lobby.Id.ToString())
            Debug.Log("Players returned to game session: " + sessionId + "lobby id: " + lobby.Id);
    }

    private void SetRanking(Lobby lobby)
    {
        if (Transport.Instance.isRankedGame)
        {
            lobby.SetData(MatchmakingKeys.GameModeKey, "true");
            lobby.SetData(MatchmakingKeys.RankingKey, facade.matchmaking.Ranking().ToString());
            Debug.Log("Set game mode to ranked. Ranking: " + facade.matchmaking.Ranking());
        }
        else
        {
            lobby.SetData(MatchmakingKeys.GameModeKey, "false");
            Debug.Log("Set game mode to unranked");
        }
    }

    private static void SetHostData(Lobby lobby)
    {
        ulong host = SteamClient.SteamId;
        lobby.SetData(MatchmakingKeys.HostKey, host.ToString());
        Debug.Log("Set host data to:" + host);
    }

    public override void InviteFriend(string friendId)
    {
        currentLobby.InviteFriend(ulong.Parse(friendId));
    }

    public override void OpenTransportInviteOverlay()
    {
        Debug.Log("Open transport invite overlay");
#if UNITY_EDITOR

#else
        if (Transport.Instance.isRankedGame) return;
        if (SteamClient.IsValid)
        {
            SteamFriends.OpenOverlay("friends");
        }
#endif
    }

    public override void KickPlayer(string id)
    {
        MyLogger.Log("Player with id: " + id, logLevel);
        if (SteamServer.IsValid)
        {
            SteamServer.EndSession(ulong.Parse(id));
        }
    }


    private List<Friend> SetActiveFriends()
    {
        activeFriends.Clear();
        var steamFriends = SteamFriends.GetFriends();
        foreach (var friend in steamFriends)
        {
            if (friend.IsOnline)
            {
                activeFriends.Add(friend);
            }
        }

        return activeFriends;
    }
}