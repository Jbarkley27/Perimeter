using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BarrierModule : MonoBehaviour
{
    [Header("Barrier Values")]
    public double currentBarrier;
    public Slider barrierSlider;
    public GameObject barrierRoot;
    public TMP_Text barrierText;


    [Header("Health Values")]
    public double currentHealth;
    public Slider healthSlider;
    public GameObject healthRoot;
    public TMP_Text healthText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentBarrier = StatsManager.Instance.GetStat(StatsManager.StatType.BARRIER);
        currentHealth = StatsManager.Instance.GetStat(StatsManager.StatType.HEALTH);
    }

    // Update is called once per frame
    void Update()
    {
        // Keep UI updated
       if (healthSlider) healthSlider.maxValue = (int)StatsManager.Instance.GetStat(StatsManager.StatType.HEALTH);
       if (barrierSlider) barrierSlider.maxValue = (int)StatsManager.Instance.GetStat(StatsManager.StatType.BARRIER);


        if (barrierSlider) barrierSlider.value = (int)currentBarrier;
        if (barrierSlider) barrierText.text = $"{(int)currentBarrier} / {(int)StatsManager.Instance.GetStat(StatsManager.StatType.BARRIER)}";

        if (healthSlider) healthSlider.value = (int)currentHealth;
        if (healthSlider) healthText.text = $"{(int)currentHealth} / {(int)StatsManager.Instance.GetStat(StatsManager.StatType.HEALTH)}";
    }

    public void ResetHealthBarrier()
    {
        currentBarrier = StatsManager.Instance.GetStat(StatsManager.StatType.BARRIER);
        currentHealth = StatsManager.Instance.GetStat(StatsManager.StatType.HEALTH);
    }


    public void TakeDamage(double amount)
    {
        // Apply damage to barrier first
        if (currentBarrier > 0)
        {

            double damageToBarrier = Mathf.Min((float)amount, (float)currentBarrier);
            currentBarrier -= damageToBarrier;
            amount -= damageToBarrier;
            barrierRoot.transform.DOPunchScale(Vector3.one * .15f, 0.15f, 10, 1)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    barrierRoot.transform.localScale = Vector3.one;
                });

            // we need to figure out if this is the first time the barrier broke/depleted
            if (currentBarrier <= 0)
            {
                Debug.Log("Barrier depleted!");
                // You can add additional effects or logic here for when the barrier breaks
            }
        }

        // If there's remaining damage, apply it to health
        if (amount > 0)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Max((float)currentHealth, 0f);
            healthRoot.transform.DOPunchScale(Vector3.one * .15f, 0.15f, 10, 1)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    healthRoot.transform.localScale = Vector3.one;
                });
        }

        // Clamp values to not exceed max
        currentBarrier = Mathf.Clamp((float)currentBarrier, 0f, (float)StatsManager.Instance.GetStat(StatsManager.StatType.BARRIER));
        currentHealth = Mathf.Clamp((float)currentHealth, 0f, (float)StatsManager.Instance.GetStat(StatsManager.StatType.HEALTH));

        // If health drops to zero, end run
        if (currentHealth <= 0)
        {
            Debug.Log("Player health depleted! Ending run.");
            EnemyManager.Instance.PlayerWonBySwarmDefeated = false;
            GameManager.Instance.EndRun();
        }

    }
}
