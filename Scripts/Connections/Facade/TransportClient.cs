using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class TransportClient : MonoBehaviour
{
    public string sessionId;
    public abstract void JoinToNewHostGame(string newHost);
    public abstract void LeaveLobby();
    public abstract Task<List<TransportLobby>> GetCreatedLobbiesBySession();
    public abstract void AcceptLobbyInvitation(string id);
    public abstract void DeclineInvitation(string lobbyId);

    public virtual void ClearLastSession()
    {
        sessionId = "";
    }
}