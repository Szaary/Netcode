public class SteamFacade : TransportFacade
{
    public override void SetupTransport()
    {
        if (login.IsValid())
        {
            networkTransport = transport.facepunch;
            networkManager.NetworkConfig.NetworkTransport = networkTransport;
        }
        else
        {
            SetUnityTransport();
        }
    }
}