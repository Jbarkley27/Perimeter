using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    public enum StatType
    {
        HEALTH,
        BARRIER,
    }

    [Header("Health/Barrier Stats")]
    public double HealthStat = 5;
    public double BarrierStat = 5;
    private double healthFlat;
    private float healthPercent;
    private double barrierFlat;
    private float barrierPercent;


    [Header("Critical Hit Chance")]
    public float critChance = 0.1f; // 10% default crit chance

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
                return HealthStat;
            case StatType.BARRIER:
                return BarrierStat;
            default:
                return 0;
        }
    }

    public void ResetModifiers()
    {
        healthFlat = 0;
        healthPercent = 0f;
        barrierFlat = 0;
        barrierPercent = 0f;
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
        }
    }
}