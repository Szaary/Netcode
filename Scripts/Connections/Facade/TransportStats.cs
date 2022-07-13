using UnityEngine;

public abstract class TransportStats : MonoBehaviour
{
    public abstract void AddStat(string aName, int stat);
    public abstract void SetStat(string aName, int stat);
    public abstract int GetStat(string stat);

    public abstract void Store();
    public abstract void Store(string statAchievementStat, int statAchievementProgress);
}