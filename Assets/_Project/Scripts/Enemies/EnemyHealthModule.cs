using UnityEngine;
using UnityEngine.UI;
using DamageNumbersPro;


public class EnemyHealthModule : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBarSlider;
    public DamageNumber damageNumberPrefab;
    public float heightOffset = 5;
    public EnemyDataStore.EnemyType enemyType;
    public Slider castTimeSlider;
    public GameObject assignedEnemy;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }


    void Update()
    {
        if (assignedEnemy)
        {
            FollowWorldSpaceEnemyPosition(assignedEnemy.transform);
            UpdateHealthBar();
        }

    }


    public void FollowWorldSpaceEnemyPosition(Transform enemyTransform)
    {
        // This is on a UI Canvas NOT in world space, so we need to convert the world position to screen position
        Vector3 worldPosition = enemyTransform.position + new Vector3(0, heightOffset, 0);
        Debug.Log("Enemy World Position: " + worldPosition);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        transform.position = screenPosition;
    }


    public void Initialize(EnemyDataStore.EnemyType type, float health)
    {
        enemyType = type;
        maxHealth = health;
        currentHealth = health;
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = currentHealth;
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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider == null)
            return;

        healthBarSlider.value = currentHealth;
    }


    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        
        // Add death logic here (e.g., play animation, drop loot, etc.)

        EnemyManager.Instance.DefeatEnemy(enemyType);

        GlassManager.Instance.CollectGlass(enemyType);

        EnemyPooler.Instance.ReturnEnemyToPool(
            assignedEnemy,
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
    }
}
