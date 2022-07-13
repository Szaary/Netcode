using System;
using UnityEngine;

public abstract class TransportLogin : MonoBehaviour
{
    private string playerName = "Player";
    
    public event Action<bool> onLoginSuccess;
    
    protected virtual void OnLoginSuccess(bool success)
    {
        onLoginSuccess?.Invoke(success);
    }

    public abstract void Login();
    public abstract bool IsValid();
    public string GetPlayerName() => playerName;
    public abstract string GetPlayerId();
    
    protected void SetPlayerName(string player)
    {
        if (player.Length > 29)
        {
            playerName = player.Remove(29, playerName.Length - 29);
        }
        Debug.Log("Set player name to: " + playerName);
        playerName= player;
    }
}