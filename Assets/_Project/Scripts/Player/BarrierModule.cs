using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BarrierModule : MonoBehaviour
{
    [Header("Barrier Values")]
    public double barrierStrength;
    public double maxBarrierStrength;
    public Slider barrierSlider;
    public GameObject barrierRoot;
    public TMP_Text barrierText;


    [Header("Health Values")]
    public double healthStrength;
    public double maxHealthStrength;
    public Slider healthSlider;
    public GameObject healthRoot;
    public TMP_Text healthText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        barrierSlider.value = (float)(barrierStrength / maxBarrierStrength);
        barrierText.text = $"{(int)barrierStrength} / {(int)maxBarrierStrength}";

        healthSlider.value = (float)(healthStrength / maxHealthStrength);
        healthText.text = $"{(int)healthStrength} / {(int)maxHealthStrength}";
    }

    public void ResetBarrier()
    {
        barrierStrength = maxBarrierStrength;
    }

    public double GetBarrierStrength()
    {
        return barrierStrength;
    }

    public double GetHealthStrength()
    {
        return healthStrength;
    }

    public void TakeDamage(double amount)
    {
        // Apply damage to barrier first
        if (barrierStrength > 0)
        {

            double damageToBarrier = Mathf.Min((float)amount, (float)barrierStrength);
            barrierStrength -= damageToBarrier;
            amount -= damageToBarrier;
            barrierRoot.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 10, 1);

            // we need to figure out if this is the first time the barrier broke/depleted
            if (barrierStrength <= 0)
            {
                Debug.Log("Barrier depleted!");
                // You can add additional effects or logic here for when the barrier breaks
            }
        }

        // If there's remaining damage, apply it to health
        if (amount > 0)
        {
            healthStrength -= amount;
            healthStrength = Mathf.Max((float)healthStrength, 0f);
            healthRoot.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 10, 1);
        }

        // Clamp values to not exceed max
        barrierStrength = Mathf.Clamp((float)barrierStrength, 0f, (float)maxBarrierStrength);
        healthStrength = Mathf.Clamp((float)healthStrength, 0f, (float)maxHealthStrength);

        // If health drops to zero, end run
        if (healthStrength <= 0)
        {
            Debug.Log("Player health depleted! Ending run.");
            GameManager.Instance.EndRun();
        }

    }
}
