using System.Collections.Generic;
using System.Linq;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamTransportAchievement : TransportAchievements
{
    [SerializeField] private LogLevel logLevel;
    [SerializeField] private AchievementLibrary achievementLibrary;
    
    protected void Awake()
    {
        SteamUserStats.OnUserStatsReceived += OnUserStatsReceived;
    }

    private void OnUserStatsReceived(SteamId userId, Result result)
    {
        var baseAchievements = WrapAchievements();
        achievementLibrary.SetState(baseAchievements);
        SteamUserStats.OnAchievementProgress += AchievementChanged;
    }

    private void AchievementChanged(Achievement ach, int currentProgress, int progress)
    {
        if (ach.State)
        {
            var baseAchievement = WrapAchievement(ach);
            achievementLibrary.SetAchievement(baseAchievement);
        }
    }

    private static List<BaseAchievement> WrapAchievements()
    {
        var baseAchievements = new List<BaseAchievement>();
        foreach (var ach in SteamUserStats.Achievements.ToList())
        {
            var baseAchievement = WrapAchievement(ach);
            baseAchievements.Add(baseAchievement);
        }

        return baseAchievements;
    }

    private static BaseAchievement WrapAchievement(Achievement ach)
    {
        var baseAchievement = ScriptableObject.CreateInstance<BaseAchievement>();
        baseAchievement.aName = ach.Name;
        baseAchievement.isAchieved = ach.State;
        return baseAchievement;
    }

    public override void Trigger(string aName)
    {
        if (!SteamClient.IsValid) return;
        
        var achievement = SteamUserStats.Achievements.FirstOrDefault(x => x.Identifier == aName);
        if (achievement.State == false)
        {
            achievement.Trigger();
            facade.stats.Store();
        }
    }
    
    public override void WipeAchievements()
    {
        SteamUserStats.ResetAll(true);
        SteamUserStats.StoreStats();
        var baseAchievements = WrapAchievements();
        achievementLibrary.SetState(baseAchievements);
        achievementLibrary.WipeProgress();
    }

    protected  void OnDestroy()
    {
        SteamUserStats.OnAchievementProgress -= AchievementChanged;
        SteamUserStats.OnUserStatsReceived -= OnUserStatsReceived;
    }
}