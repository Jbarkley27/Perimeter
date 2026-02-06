using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    public enum StatType
    {
        HEALTH,
        BARRIER,
        CRIT_CHANCE
    }

    [Header("Health/Barrier Stats")]
    public double HealthStat = 5;
    public double BarrierStat = 5;
    private double healthFlat;
    private float healthPercent;
    private double barrierFlat;
    private float barrierPercent;


    
    [Header("Critical Hit Chance")]
    public float critChance = 0.1f; // base

    private float critFlat;
    private float critPercent;


    [Header("Sector Modifiers")]
    private double healthFlatSector;
    private float healthPercentSector;
    private double barrierFlatSector;
    private float barrierPercentSector;

    private float critFlatSector;
    private float critPercentSector;




    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    
    public double GetStat(StatType stat)
    {
        switch (stat)
        {
            case StatType.HEALTH:
                return (HealthStat + healthFlat + healthFlatSector) * (1 + healthPercent + healthPercentSector);
            case StatType.BARRIER:
                return (BarrierStat + barrierFlat + barrierFlatSector) * (1 + barrierPercent + barrierPercentSector);
            case StatType.CRIT_CHANCE:
                return Mathf.Clamp01(critChance + critFlat + critFlatSector) * (1 + critPercent + critPercentSector);
            default:
                return 0;
        }
    }


    // Backwards compatibility for existing callers.
    public void ResetModifiers()
    {
        ResetSkillModifiers();
    }

    // Resets only skill tree modifiers.
    public void ResetSkillModifiers()
    {
        healthFlat = 0;
        healthPercent = 0f;
        barrierFlat = 0;
        barrierPercent = 0f;
        critFlat = 0f;
        critPercent = 0f;
    }

    // Resets only sector modifiers.
    public void ResetSectorModifiers()
    {
        healthFlatSector = 0;
        healthPercentSector = 0f;
        barrierFlatSector = 0;
        barrierPercentSector = 0f;
        critFlatSector = 0f;
        critPercentSector = 0f;
    }


    public void ApplyModifier(StatType stat, double flat, float percent)
    {
        switch (stat)
        {
            case StatType.HEALTH:
                healthFlat += flat;
                healthPercent += percent;
                break;
            case StatType.BARRIER:
                barrierFlat += flat;
                barrierPercent += percent;
                break;
            case StatType.CRIT_CHANCE:
                critFlat += (float)flat;
                critPercent += percent;
                break;
        }
    }

    // Applies a modifier from the sector system (separate from skill tree).
    public void ApplySectorModifier(StatType stat, double flat, float percent)
    {
        switch (stat)
        {
            case StatType.HEALTH:
                healthFlatSector += flat;
                healthPercentSector += percent;
                break;
            case StatType.BARRIER:
                barrierFlatSector += flat;
                barrierPercentSector += percent;
                break;
            case StatType.CRIT_CHANCE:
                critFlatSector += (float)flat;
                critPercentSector += percent;
                break;
        }
    }

}