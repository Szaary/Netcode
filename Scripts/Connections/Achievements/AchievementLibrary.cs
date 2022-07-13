using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Achievements", menuName = "ScriptableObjects/Achievements")]
public class AchievementLibrary : ScriptableObject
{
    [SerializeField] private LogLevel logLevel;

    public List<BaseAchievement> achievements;
   
    public void Trigger(BaseAchievement baseAchievement)
    {
        if (baseAchievement.isAchieved) return;
        Transport.Instance.facade.achievements.Trigger(baseAchievement.aName);
        baseAchievement.isAchieved = true;
    }

    public void AddStat(StatAchievement statAchievement, int value)
    {
        if (statAchievement.isAchieved) return;
        Transport.Instance.facade.stats.AddStat(statAchievement.stat, value);
        statAchievement.progress += value;
        if (statAchievement.progress >= statAchievement.max)
        {
            Transport.Instance.facade.stats.Store(statAchievement.stat, statAchievement.progress);
        }
    }

    public void SetState(List<BaseAchievement> achievements)
    {
        foreach (var achievement in achievements)
        {
            SetAchievement(achievement);
        }
    }

    public void SetAchievement(BaseAchievement achievement)
    {
        foreach (var ach in achievements)
        {
            UpdateAchievementStatus(achievement, ach);
        }
    }
    private void UpdateAchievementStatus(BaseAchievement achievement, BaseAchievement baseAchievement)
    {
        if (achievement.aName == baseAchievement.aName)
        {
            baseAchievement.isAchieved = achievement.isAchieved;
            if (achievement.isAchieved) MyLogger.Log($"Set achievement  {achievement.aName} status to isAchieved", logLevel);

            if (baseAchievement is StatAchievement stat)
            {
                stat.progress = Transport.Instance.facade.stats.GetStat(stat.stat);
            }
        }
    }

    public void WipeProgress()
    {
        foreach (var ach in achievements)
        {
            if (ach is StatAchievement stat)
            {
                stat.progress = 0;
            }
        }
    }
}