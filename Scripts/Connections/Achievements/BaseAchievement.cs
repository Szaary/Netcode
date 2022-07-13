using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseAchievement_", menuName = "ScriptableObjects/BaseAchievement")]
public class BaseAchievement : ScriptableObject
{
    public string aName;
    public bool isAchieved;
}