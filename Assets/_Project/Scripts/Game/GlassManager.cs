using TMPro;
using UnityEngine;

public class GlassManager : MonoBehaviour
{
    public static GlassManager Instance;
    public double glassShardsThisRun = 0;
    public double totalGlassShardsCollected = 0;
    public TMP_Text glassCollectedTextInGame;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found a Glass Manager object, destroying new one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddGlass(double amount)
    {
        glassShardsThisRun += amount;
        totalGlassShardsCollected += amount;
        Debug.Log($"Collected {amount} glass shards. Total: {glassShardsThisRun}");
        if (glassCollectedTextInGame)
        {
            glassCollectedTextInGame.text = $"{(int)glassShardsThisRun}";
        }
    }

    public double GetCurrentGlassShards()
    {
        return glassShardsThisRun;
    }

    public double GetTotalGlassShardsCollected()
    {
        return totalGlassShardsCollected;
    }

    public void ResetGlassThisRun()
    {
        glassShardsThisRun = 0;
    }


    public void CollectGlass(EnemyDataStore.EnemyType enemyType)
    {
        double rewardAmount = EnemyDataStore.Instance.GetGlassRewardForEnemyType(enemyType);
        AddGlass(rewardAmount);
    }
}