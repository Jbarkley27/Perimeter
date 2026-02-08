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
        totalGlassShardsCollected = Random.Range(60, 70); // For testing purposes
    }

    void Update()
    {
        if (glassCollectedTextInGame)
        {
            glassCollectedTextInGame.text = GetTotalGlassShardsCollectedFormatted();
        }
    }

    public void AddGlass(double amount)
    {
        glassShardsThisRun += amount;
        totalGlassShardsCollected += amount;
        // Debug.Log($"Collected {amount} glass shards. Total: {glassShardsThisRun}");
        if (glassCollectedTextInGame)
        {
            glassCollectedTextInGame.text = GetTotalGlassShardsCollectedFormatted();
        }

        // kill any existing animations
        glassIcon.transform.DOKill();

        glassIcon.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 10, 1)
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

    public string GetCurrentGlassShardsFormatted()
    {
        return FormatGlass(glassShardsThisRun);
    }

    public double GetTotalGlassShardsCollected()
    {
        return totalGlassShardsCollected;
    }

    public string GetTotalGlassShardsCollectedFormatted()
    {
        return FormatGlass(totalGlassShardsCollected);
    }

    public void ResetGlassThisRun()
    {
        glassShardsThisRun = 0;
    }


    public bool CanAffordNodePurchase(int cost)
    {
        return totalGlassShardsCollected >= cost;
    }

    public bool CanAffordGlass(double amount)
    {
        return totalGlassShardsCollected >= amount;
    }


    public void CollectGlass(EnemyDataStore.EnemyType enemyType)
    {
        CollectGlass(enemyType, Element.Kinetic);
    }


    public void CollectGlass(EnemyDataStore.EnemyType enemyType, Element element)
    {
        double rewardAmount = EnemyDataStore.Instance.GetGlassRewardForEnemyType(enemyType);

        float mult = SectorManager.Instance != null
            ? SectorManager.Instance.GetGlassEarnedMultiplier()
            : 1f;

        float elementMult = SectorManager.Instance != null
            ? SectorManager.Instance.GetModifierMultiplier(SectorModifierEffectType.ElementGlassMultiplier, element)
            : 1f;

        AddGlass(rewardAmount * mult * elementMult);
    }

    


    public bool SpendGlass(double amount)
    {
        if (totalGlassShardsCollected >= amount)
        {
            totalGlassShardsCollected -= amount;
            if (glassCollectedTextInGame)
            {
                glassCollectedTextInGame.text = GetTotalGlassShardsCollectedFormatted();
            }
            return true;
        }
        return false;
    }

    private static string FormatGlass(double amount)
    {
        if (amount < 0)
            amount = 0;
        return System.Math.Floor(amount).ToString("0");
    }
}
