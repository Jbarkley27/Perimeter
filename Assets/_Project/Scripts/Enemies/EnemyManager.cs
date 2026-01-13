using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    public int totalEnemiesDefeated = 0;
    public int totalEnemiesDeafeatedThisRun = 0;
    public double totalDamageDealtToEnemies = 0;
    public double totalDamageDealtToEnemiesThisRun = 0;
    public double requiredDamageToWin = 10.0;
    public int enemyAttackPool; // used to limit number of enemies that can attack at once

    public Dictionary<EnemyDataStore.EnemyType, int> enemiesDefeatedByTypeThisRun = new Dictionary<EnemyDataStore.EnemyType, int>()
    {
        { EnemyDataStore.EnemyType.BASIC, 0 },
        { EnemyDataStore.EnemyType.BOMBER, 0 },
        { EnemyDataStore.EnemyType.HEALER, 0 },
        { EnemyDataStore.EnemyType.SNIPER, 0 },
        { EnemyDataStore.EnemyType.TANK, 0 },
    };


    public Dictionary<EnemyDataStore.EnemyType, int> enemiesDefeatedByTypeTotal = new Dictionary<EnemyDataStore.EnemyType, int>()
    {
        [EnemyDataStore.EnemyType.BASIC] = 0,
        [EnemyDataStore.EnemyType.BOMBER] = 0,
        [EnemyDataStore.EnemyType.HEALER] = 0,
        [EnemyDataStore.EnemyType.SNIPER] = 0,
        [EnemyDataStore.EnemyType.TANK] = 0,
    };

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found an EnemyManager object, destroying new one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void DefeatEnemy(EnemyDataStore.EnemyType enemyType)
    {
        totalEnemiesDefeated++;
        totalEnemiesDeafeatedThisRun++;
        
        // verify enemyType is in the dictionary
        if (enemiesDefeatedByTypeThisRun.ContainsKey(enemyType))
        {
            enemiesDefeatedByTypeThisRun[enemyType]++;
        }
        else
        {
            enemiesDefeatedByTypeThisRun[enemyType] = 1;
        }

        // verify enemyType is in the total dictionary
        if (enemiesDefeatedByTypeTotal.ContainsKey(enemyType))
        {
            enemiesDefeatedByTypeTotal[enemyType]++;
        }
        else
        {
            enemiesDefeatedByTypeTotal[enemyType] = 1;
        }
    }

    public void Reset()
    {
        totalEnemiesDeafeatedThisRun = 0;

        // loop through keys and reset values to 0
        List<EnemyDataStore.EnemyType> keys = new List<EnemyDataStore.EnemyType>(enemiesDefeatedByTypeThisRun.Keys);
        foreach (var key in keys)
        {
            enemiesDefeatedByTypeThisRun[key] = 0;
        }

        totalDamageDealtToEnemiesThisRun = 0;
    }

    public double GetTotalDamageDealtToEnemiesThisRun()
    {
        return totalDamageDealtToEnemiesThisRun;
    }

    public void AddDamageDealtToEnemies(double amount)
    {
        totalDamageDealtToEnemies += amount;
        totalDamageDealtToEnemiesThisRun += amount;

        if (totalDamageDealtToEnemiesThisRun >= requiredDamageToWin)
        {
            // Player has dealt enough damage to win
            Debug.Log("Required damage dealt! You win!");
            GameManager.Instance.EndRun();
        }
    }


    public bool HasPlayerDealtRequiredDamageToWin()
    {
        return totalDamageDealtToEnemiesThisRun >= requiredDamageToWin;
    }


    public bool RequestEnemyAttackPermission(EnemyDataStore.EnemyType enemyType)
    {
        int attackCost = EnemyDataStore.Instance.GetAttackCostForEnemyType(enemyType);
        if (enemyAttackPool >= attackCost)
        {
            enemyAttackPool -= attackCost;
            return true;
        }
        return false;
    }

    public void ReturnEnemyAttackPermission(EnemyDataStore.EnemyType enemyType)
    {
        int attackCost = EnemyDataStore.Instance.GetAttackCostForEnemyType(enemyType);
        enemyAttackPool += attackCost;
    }
}