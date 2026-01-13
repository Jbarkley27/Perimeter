using System.Collections.Generic;
using UnityEngine;

public class EnemyDataStore : MonoBehaviour
{
     public enum EnemyType
    {
        BASIC,
        BOMBER,
        SNIPER,
        TANK,
        HEALER
    }


    public static EnemyDataStore Instance { get; private set; }
    public List<EnemyBaseData> enemyGlassRewards = new List<EnemyBaseData>()
    {
        new EnemyBaseData(EnemyType.BASIC, 5, 1),
        new EnemyBaseData(EnemyType.BOMBER, 10, 1),
        new EnemyBaseData(EnemyType.HEALER, 8, 1),
        new EnemyBaseData(EnemyType.SNIPER, 12, 1),
        new EnemyBaseData(EnemyType.TANK, 15, 1),
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

    public int GetGlassRewardForEnemyType(EnemyType enemyType)
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

    public int GetAttackCostForEnemyType(EnemyType enemyType)
    {
        foreach (var reward in enemyGlassRewards)
        {
            if (reward.enemyType == enemyType)
            {
                return reward.attackCost;
            }
        }
        return 0; // Default if not found
    }
}

[System.Serializable]
public struct EnemyBaseData
{
    public EnemyDataStore.EnemyType enemyType;
    public int glassRewardAmount;
    public int attackCost;

    public EnemyBaseData(EnemyDataStore.EnemyType type, int glassReward, int attackCost)
    {
        this.enemyType = type;
        this.glassRewardAmount = glassReward;
        this.attackCost = attackCost;
    }
}
