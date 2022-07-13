using System.Collections;
using Steamworks;
using UnityEngine;

public class SteamStats : TransportStats
{
    private void Start()
    {
        if (!SteamClient.IsValid)
        {
            Debug.Log("Client is not valid");
            return;
        }

        SteamUserStats.RequestCurrentStats();
    }

    public override int GetStat(string stat)
    {
        if (SteamClient.IsValid) return SteamUserStats.GetStatInt(stat);
        return 0;
    }

    public override void AddStat(string aName, int stat)
    {
        if (SteamClient.IsValid) SteamUserStats.AddStat(aName, stat);
    }

    public override void SetStat(string aName, int stat)
    {
        if (SteamClient.IsValid) SteamUserStats.SetStat(aName, stat);
    }


    public override void Store(string statAchievementStat, int statAchievementProgress)
    {
        Store();
    }

    public override void Store()
    {
        if (SteamClient.IsValid) return;
        StartCoroutine(StoreStat());
    }

    private IEnumerator StoreStat()
    {
        while (!SteamUserStats.StoreStats())
        {
            Debug.LogWarning("Waiting for steam connection to set achievement");
            yield return new WaitForSeconds(60);
        }
    }

    private void OnDestroy()
    {
        if (SteamClient.IsValid) SteamUserStats.StoreStats();
    }
}