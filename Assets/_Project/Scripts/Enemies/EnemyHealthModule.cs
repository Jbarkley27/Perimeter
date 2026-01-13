using UnityEngine;
using UnityEngine.UI;
using DamageNumbersPro;


public class EnemyHealthModule : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthBarSlider;
    public DamageNumber damageNumberPrefab;
    public float heightOffset = 5;
    public EnemyPooler.EnemyType enemyType;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int amount)
    {
        // only send the net damage to SignalManager
        int actualDamage = (int)Mathf.Min(currentHealth, amount);

        EnemyManager.Instance.AddDamageDealtToEnemies(actualDamage);

        Debug.Log($"{gameObject.name} took {amount} damage.");
        Debug.Log("Damage Signal Received" + actualDamage);
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        
        // Add death logic here (e.g., play animation, drop loot, etc.)

        EnemyManager.Instance.DefeatEnemy(enemyType);

        GlassManager.Instance.CollectGlass(enemyType);

        EnemyPooler.Instance.ReturnEnemyToPool(
            gameObject,
            enemyType
        );
    }


    public void ShowDamageUI(int damage)
    {
        Vector3 offsetVec = new Vector3(transform.position.x, heightOffset, transform.position.z);
        if (damageNumberPrefab) damageNumberPrefab.Spawn(offsetVec, damage.ToString());
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }
}
