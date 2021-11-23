using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;


public class SteamServer : SingletonMB<SteamServer>
{
    [SerializeField] private ServerGameNetPortal serverGameNetPortal;
    private void Start()
    {
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyMemberJoined += LobbyMemberJoined;
    }

    private void LobbyMemberJoined(Lobby lobby, Friend steamId)
    {
        Debug.Log("Joined:" + steamId);
        
    }

    public void StartSteamHost()
    {
        SteamMatchmaking.CreateLobbyAsync(serverGameNetPortal.maxPlayers);
    }

    private void OnLobbyCreated(Result callback, Lobby lobby)
    {
        lobby.SetPublic();
        if (callback != Result.OK)
        {
            Debug.Log("Lobby Not Created");
        }
        else
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Game Hosted");
        }
    }
}