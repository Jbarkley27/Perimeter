using System.Collections.Generic;
using UnityEngine;

public class EnemyDataStore : MonoBehaviour
{
    public static EnemyDataStore Instance { get; private set; }
    public List<EnemyGlassReward> enemyGlassRewards = new List<EnemyGlassReward>()
    {
        new EnemyGlassReward(EnemyPooler.EnemyType.BASIC, 1),
        new EnemyGlassReward(EnemyPooler.EnemyType.BOMBER, 2),
        new EnemyGlassReward(EnemyPooler.EnemyType.HEALER, 4),
        new EnemyGlassReward(EnemyPooler.EnemyType.SNIPER, 3),
        new EnemyGlassReward(EnemyPooler.EnemyType.TANK, 5),
    };

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetGlassRewardForEnemyType(EnemyPooler.EnemyType enemyType)
    {
        foreach (var reward in enemyGlassRewards)
        {
            if (reward.enemyType == enemyType)
            {
                return reward.glassRewardAmount;
            }
        }
        return 0; // Default if not found
    }
}

[System.Serializable]
public struct EnemyGlassReward
{
    public EnemyPooler.EnemyType enemyType;
    public int glassRewardAmount;

    public EnemyGlassReward(EnemyPooler.EnemyType type, int amount)
    {
        enemyType = type;
        glassRewardAmount = amount;
    }
}