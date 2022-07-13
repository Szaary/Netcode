using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Achievement_", menuName = "ScriptableObjects/Achievement")]
public class StatAchievement : BaseAchievement
{
    public string stat;
    public int max;

    public int progress;
}