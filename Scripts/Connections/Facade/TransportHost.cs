using System.Collections.Generic;
using UnityEngine;

public abstract class TransportHost : MonoBehaviour
{
    public string sessionId;
    protected readonly List<string> connectedIds = new List<string>();
    protected bool isPrivateLobby;
    
    public virtual void StartTransportHost()
    {
        sessionId = "";
    }
    public abstract void StartTransportHostLastSession();

    public virtual void StartTransportHostLastSession(string clientId)
    {
        connectedIds.Add(clientId);
    }


    public bool GetLobbyVisibility() => isPrivateLobby;
    public abstract List<TransportFriend> GetActiveFriends();
    public abstract void InviteFriend(string friendId);
    public abstract void OpenTransportInviteOverlay();
    public abstract void KickPlayer(string id);

    public virtual void ClearLastSession() => sessionId = "";
    public abstract void LeaveLobbyAfterReconnect(string clientId);
    public abstract void SetPrivateLobby(bool isActive);
}