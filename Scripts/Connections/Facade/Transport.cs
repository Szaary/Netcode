using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public enum TransportType
{
    Steam,
    Epic,
    Gog
}

public class Transport : SingletonMB<Transport>
{
    public TransportType transportType;

    [SerializeField] private TransportFacade steam;
    [SerializeField] private TransportFacade epic;
    [SerializeField] private TransportFacade gog;

    public TransportFacade facade;

    [Header("References to transport")] public NetworkTransport facepunch;
    public NetworkTransport unity;

    public bool isRankedGame;

    public void StartTransportHost(bool isRanked)
    {
        isRankedGame = isRanked;
        facade.host.StartTransportHost();
    }


    #region Matchmaking

    public void PlayRanked()
    {
        ShutdownServer();

        MatchmakingParameters parameters = new MatchmakingParameters()
        {
            isRanked = true,
            mapName = ""
        };
        facade.matchmaking.QueryLobby(parameters);
    }

    public void PlayQuickMatch()
    {
        ShutdownServer();

        MatchmakingParameters parameters = new MatchmakingParameters()
        {
            isRanked = false,
            mapName = ""
        };
        facade.matchmaking.QueryLobby(parameters);
    }


    public void PlayCustomMatch(string terrainLevelName)
    {
        ShutdownServer();

        MatchmakingParameters parameters = new MatchmakingParameters()
        {
            mapName = terrainLevelName,
            isRanked = false
        };
        facade.matchmaking.QueryLobby(parameters);
    }

    public void SetNewRating(bool victory)
    {
        if (isRankedGame) facade.matchmaking.SetRanking(victory);
    }

    private static void ShutdownServer()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    #endregion


    public void SetSessionId(string id)
    {
        facade.client.sessionId = id;
        facade.host.sessionId = id;
    }

    public void ClearSessionData()
    {
        facade.client.ClearLastSession();
        facade.host.ClearLastSession();
    }


    public void SetupTransport()
    {
        if (transportType == TransportType.Steam)
        {
            steam.SetupTransport();
        }
        else if (transportType == TransportType.Epic)
        {
            epic.SetupTransport();
        }
        else if (transportType == TransportType.Gog)
        {
            epic.SetupTransport();
        }
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        if (BuildPipeline.isBuildingPlayer) return;

        SetupTransport();
        if (transportType == TransportType.Steam)
        {
            steam.gameObject.SetActive(true);
            epic.gameObject.SetActive(false);
            gog.gameObject.SetActive(false);

            facepunch.gameObject.SetActive(true);

            facade = steam;
        }
        else if (transportType == TransportType.Epic)
        {
            epic.gameObject.SetActive(true);
            steam.gameObject.SetActive(false);
            gog.gameObject.SetActive(false);

            facepunch.gameObject.SetActive(false);

            facade = epic;
        }
        else if (transportType == TransportType.Gog)
        {
            epic.gameObject.SetActive(false);
            steam.gameObject.SetActive(false);
            gog.gameObject.SetActive(true);

            facepunch.gameObject.SetActive(false);

            facade = gog;
        }
    }


#endif
}