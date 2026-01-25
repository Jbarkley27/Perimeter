using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    [Header("Health/Barrier Stats")]
    public int maxHealth = 5;
    public int currentHealth;
    public int maxBarrier = 5;
    public int currentBarrier;

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

        currentHealth = maxHealth;
    }
}