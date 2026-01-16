using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    private Transform player;
    private NavMeshAgent agent;
    private PositionRing ring;
    public EnemyDataStore.EnemyType enemyType;
    public EnemyHealthModule healthModule;
    public Transform enemyBodyTransform;

    [Header("Randomized Agent Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float minAccel = 6f;
    public float maxAccel = 12f;
    public float minStoppingDistance = 2f;
    public float maxStoppingDistance = 4f;
    public float statUpdateInterval = 2.5f; // seconds

    [Header("Combat")]
    public float attackRange = 3f;
    public bool inAttackRange { get; private set; }
    public AttackRange rangePreference = AttackRange.Mid;
    public double damage; // TODO: integrate with EnemyDataStore DAMAGE/HEALTH/ETC should come from there which will scale with wave number and the economy
    public float attackCooldown = 1.5f;
    public float attackCastTime = 0.5f;
    public Slider castTimeSlider;
    public bool isAttacking = false;
    public bool isOnCooldown = false;
    public EnemyAttackLibrary.EnemyAttackID enemyAttackID;
    public double BaseHealth;

    [Header("Movement")]
    public float slotReassignInterval = 2f; 
    public float slotReassignMinInterval = 1f;
    public float slotReassignMaxInterval = 3f;
    private Vector3 currentSlot;
    private float slotTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // we handle rotation manually
    }



    private void OnEnable()
    {
        player = GlobalDataStore.Instance.Player.transform;
        ring = player.GetComponent<PositionRing>();

        PickRandomSlot();
        InvokeRepeating(nameof(RandomizeAgentStats), 0f, statUpdateInterval);
        
        ResetEnemy();
    }


    public void SetupHealthModule()
    {
        // Debug.Log(EnemyManager.Instance + "Enemy Manager");
        // Debug.Log(EnemyManager.Instance.EnemyHealthModulePrefab + "Prefab");
        // Debug.Log(EnemyManager.Instance.EnemyHealthModuleParent + "Parent");
        // Debug.Log(healthModule + "Current Module");


        if (!healthModule && EnemyManager.Instance.EnemyHealthModulePrefab != null
            && EnemyManager.Instance.EnemyHealthModuleParent != null
            && EnemyManager.Instance) 
        {
            Debug.Log("Setting up health module...");
            GameObject healthModuleGO = Instantiate(
                EnemyManager.Instance.EnemyHealthModulePrefab,
                EnemyManager.Instance.EnemyHealthModuleParent
            );

            healthModule = healthModuleGO.GetComponent<EnemyHealthModule>();
            castTimeSlider = healthModule.castTimeSlider;
            healthModule.assignedEnemy = this.gameObject;
            castTimeSlider.gameObject.SetActive(false);
            healthModule.Initialize(enemyType, (int)BaseHealth);
        }
    }



    private void Update()
    {
        if (!player) return;
        if (GameManager.Instance.GamePaused) return;

        if (healthModule == null)
        {
            Debug.Log("Health module missing, setting up...");
            SetupHealthModule();
        }

        // Move toward current slot
        agent.SetDestination(currentSlot);

        // Look at player, not movement direction
        Vector3 lookDir = (player.position - transform.position);
        lookDir.y = 0;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(lookDir),
            Time.deltaTime * 4f
        );

        // Reassign slot periodically
        slotTimer += Time.deltaTime;
        if (slotTimer >= slotReassignInterval)
        {
            slotTimer = 0f;
            PickRandomSlot();
        }

        // Attack range logic
        float dist = Vector3.Distance(transform.position, player.position);
        inAttackRange = dist <= attackRange;
        // agent.isStopped = inAttackRange;
        CanAttackPlayer();
    }



    private void RandomizeAgentStats()
    {
        agent.speed = Random.Range(minSpeed, maxSpeed);
        agent.acceleration = Random.Range(minAccel, maxAccel);
        agent.stoppingDistance = Random.Range(minStoppingDistance, maxStoppingDistance);

        slotReassignInterval = Random.Range(slotReassignMinInterval, slotReassignMaxInterval);
    }


    public bool InAttackRange()
    {
        return inAttackRange;
    }


    private void PickRandomSlot()
    {
        currentSlot = ring.GetRandomSlotWithinDistance(rangePreference, transform.position, currentSlot);
    }



    private void OnDisable()
    {
        // Check if the agent was attacking, if so return its attack permission
        if (isAttacking)
        {
            EnemyManager.Instance.ReturnEnemyAttackPermission(enemyType);
            isAttacking = false;
        }


        // Reset NavMeshAgent
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            CancelInvoke(nameof(RandomizeAgentStats));
        }

        // Reset other stuff
        isOnCooldown = false;
        if (castTimeSlider) castTimeSlider.gameObject.SetActive(false);
        if (healthModule) Destroy(healthModule.gameObject);
        healthModule = null;
    }


    public void ResetEnemy()
    {
        isAttacking = false;
        isOnCooldown = false;
        slotTimer = 0f;
        PickRandomSlot();
        SetupHealthModule();
    }


    public void CanAttackPlayer()
    {
        // Debug.Log("Checking if enemy can attack player...");
        if (InAttackRange()
            && !isAttacking
            && !isOnCooldown)
        {
            Debug.Log($"{gameObject.name} is attempting to attack the player.");
            // Start attack coroutine
            if (EnemyManager.Instance.RequestEnemyAttackPermission(enemyType))
                StartCoroutine(EnemyAttackCoroutine());
        }
    }

    private IEnumerator EnemyAttackCoroutine()
    {
        isAttacking = true;
        castTimeSlider.gameObject.SetActive(true);
        castTimeSlider.value = 0f;

        float elapsed = 0f;
        while (elapsed < attackCastTime)
        {
            elapsed += Time.deltaTime;
            castTimeSlider.value = Mathf.Clamp01(elapsed / attackCastTime);
            yield return null;
        }

        // pause movement briefly to simulate attack impact
        agent.isStopped = true;



        // animate slider
        castTimeSlider.gameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                castTimeSlider.gameObject.transform.localScale = Vector3.one;
                castTimeSlider.gameObject.SetActive(false);
            });



        // Perform attack
        StartCoroutine(
            EnemyAttackLibrary.Instance.PerformAttack(
                enemyAttackID,
                this,
                new Transform[] { enemyBodyTransform } // for now, just use the body transform as the projectile source
            )
        );


        // Deal damage to player
       // later add projectile/attaack system that shoots a new EnemyProjectile toward the player
                // animate enemy body
        enemyBodyTransform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 10, 1)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                enemyBodyTransform.localScale = Vector3.one;
            });


        // wait a bit before allowing movement again
        yield return new WaitForSeconds(0.2f);
        agent.isStopped = false;


        isAttacking = false;
        isOnCooldown = true;

        yield return new WaitForSeconds(Random.Range(0.5f, 2f)); // small delay before returning attack permission
        EnemyManager.Instance.ReturnEnemyAttackPermission(enemyType);

        // Start cooldown
        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
    }
}


public enum AttackRange
{
    Close,
    Mid,
    Long
}