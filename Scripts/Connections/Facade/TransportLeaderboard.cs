using System.Collections.Generic;
using UnityEngine;

public abstract class TransportLeaderboard : MonoBehaviour
{
    public List<PlayerScore> worldScores = new();
    public List<PlayerScore> friendsScores = new();
    public PlayerScore playerScore = new();
    public abstract void SetLeaderboardScore(int score);
}