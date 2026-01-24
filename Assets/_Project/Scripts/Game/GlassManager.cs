using DG.Tweening;
using TMPro;
using UnityEngine;

public class GlassManager : MonoBehaviour
{
    public static GlassManager Instance;
    public double glassShardsThisRun = 0;
    public double totalGlassShardsCollected = 0;
    public TMP_Text glassCollectedTextInGame;
    public GameObject glassIcon;

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

    void Start()
    {
        if (glassCollectedTextInGame)
        {
            glassCollectedTextInGame.text = "0";
        }

        totalGlassShardsCollected = Random.Range(10, 50); // For testing purposes
    }

    public void AddGlass(double amount)
    {
        glassShardsThisRun += amount;
        totalGlassShardsCollected += amount;
        // Debug.Log($"Collected {amount} glass shards. Total: {glassShardsThisRun}");
        if (glassCollectedTextInGame)
        {
            glassCollectedTextInGame.text = $"{(int)totalGlassShardsCollected}";
        }

        glassIcon.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                glassIcon.transform.localScale = Vector3.one;
            });
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


    public bool CanAffordNodePurchase(int cost)
    {
        return totalGlassShardsCollected >= cost;
    }


    public void CollectGlass(EnemyDataStore.EnemyType enemyType)
    {
        double rewardAmount = EnemyDataStore.Instance.GetGlassRewardForEnemyType(enemyType);
        AddGlass(rewardAmount);
    }
}