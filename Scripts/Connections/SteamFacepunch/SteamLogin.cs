using Steamworks;
using UnityEngine;


//Facepunch transport handle initialization and login on its own. Sending only event for other systems.
public class SteamLogin : TransportLogin
{
    private void Awake()
    {
        SteamServer.OnSteamServersConnected += OnSteamServersConnected;
    }
    
    public override void Login()
    {
        SetPlayerName(SteamClient.Name);
        OnLoginSuccess(SteamClient.IsValid);
    }

    public override bool IsValid()
    {
        return SteamClient.IsValid;
    }

    private void OnSteamServersConnected()
    {
        OnLoginSuccess(true);
    }

    public override string GetPlayerId()
    {
        if(SteamClient.IsValid) return SteamClient.SteamId.ToString();
        Debug.Log("Steam client is not valid, returning Player");
        return "Player";
    }
}