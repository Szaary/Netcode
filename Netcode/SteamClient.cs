using System;
using System.Linq;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;


public class SteamClient : SingletonMB<SteamClient>
{
    [SerializeField] private FacepunchTransport facepunchTransport;
    
    private void Start()
    {
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvited;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
    }

    private void GameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        SteamMatchmaking.JoinLobbyAsync(lobby.Id);
        Debug.Log("On Join Requested");
    }


    private void OnLobbyInvited(Friend host, Lobby lobby)
    {
        facepunchTransport.targetSteamId = host.Id;

        SteamMatchmaking.JoinLobbyAsync(lobby.Id);
        Debug.Log("OnLobbyInvited");
    }

    private void LobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Hit Lobby Entered by server");
            return;
        }
        
        Debug.Log("Lobby Entered by client.");
        var host = lobby.Members.First().Id;
        facepunchTransport.targetSteamId = host;

        ClientGameNetPortal.Instance.ConnectClient(GameNetPortal.Instance);
    }
}