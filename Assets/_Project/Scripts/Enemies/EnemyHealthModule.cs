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
    private Element lastHitElement = Element.Kinetic;


    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }


    void Update()
    {
        AutoDestroyIfNoEnemyAssigned();

        if (assignedEnemy)
        {
            FollowWorldSpaceEnemyPosition(assignedEnemy.transform);
            UpdateHealthBar();
        }
    }


    public void AutoDestroyIfNoEnemyAssigned()
    {
        if (assignedEnemy == null || !assignedEnemy.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }
    }


    public void FollowWorldSpaceEnemyPosition(Transform enemyTransform)
    {
        if (enemyTransform == null || Camera.main == null)
            return;

        Vector3 worldPosition = enemyTransform.position + new Vector3(0, heightOffset, 0);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        transform.position = screenPosition;
    }



    public void Initialize(EnemyDataStore.EnemyType type, float health)
    {
        enemyType = type;

        // Apply sector difficulty + modifier scaling.
        float scaledHealth = health;
        if (SectorManager.Instance != null)
            scaledHealth *= SectorManager.Instance.GetEnemyHealthMultiplier();

        maxHealth = scaledHealth;
        currentHealth = scaledHealth;

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
    }




    public void TakeDamage(int amount)
    {
        ApplyDamage(amount, Element.Kinetic);
    }

    public void TakeDamage(int amount, Element element)
    {
        ApplyDamage(amount, element);
    }

    private void ApplyDamage(int amount, Element element)
    {
        lastHitElement = element;

        int actualDamage = (int)Mathf.Min(currentHealth, amount);

        EnemyManager.Instance.AddDamageDealtToEnemies(actualDamage);

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
        if (EnemyManager.Instance == null || EnemyPooler.Instance == null)
            return;
        
        // Add death logic here (e.g., play animation, drop loot, etc.)

        if (castTimeSlider != null)
            castTimeSlider.gameObject.SetActive(false);


        EnemyManager.Instance.DefeatEnemy(enemyType);

        if (GlassManager.Instance != null)
            GlassManager.Instance.CollectGlass(enemyType, lastHitElement);


        if (assignedEnemy != null)
        {
            EnemyPooler.Instance.ReturnEnemyToPool(
                assignedEnemy,
                enemyType
            );
        }
    }


    public void ShowDamageUI(int damage)
    {
        if (damageNumberPrefab == null)
            return;

        Vector3 offsetVec = new Vector3(transform.position.x, heightOffset, transform.position.z);
        damageNumberPrefab.Spawn(offsetVec, damage.ToString());
    }


    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }


    
}
