using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public abstract class TransportFacade : MonoBehaviour
{
    [SerializeField] protected Transport transport;
    
    [SerializeField] protected NetworkManager networkManager;
    public NetworkTransport networkTransport;
    
    
    public TransportHost host;
    public TransportClient client;
    public TransportMatchmaking matchmaking;
    public TransportStats stats;
    public TransportLeaderboard leaderboard;
    public TransportLogin login;
    public TransportAchievements achievements;
    
    public abstract void SetupTransport();
    
    protected void SetUnityTransport()
    {
        networkTransport = transport.unity;
        if (networkTransport is UnityTransport unityTransport)
        {
            unityTransport.SetConnectionData("127.0.0.1", (ushort) 7777);
            networkManager.NetworkConfig.NetworkTransport = networkTransport;
        }
    }
}