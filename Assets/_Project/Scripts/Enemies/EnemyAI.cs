using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    private Transform player;
    private NavMeshAgent agent;
    private PositionRing ring;

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
    }

    private void Update()
    {
        if (!player) return;

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
        agent.isStopped = inAttackRange;
    }

    private void RandomizeAgentStats()
    {
        agent.speed = Random.Range(minSpeed, maxSpeed);
        agent.acceleration = Random.Range(minAccel, maxAccel);
        agent.stoppingDistance = Random.Range(minStoppingDistance, maxStoppingDistance);
        slotReassignInterval = Random.Range(slotReassignMinInterval, slotReassignMaxInterval);
    }

    private void PickRandomSlot()
    {
        currentSlot = ring.GetRandomSlotWithinDistance(rangePreference, transform.position, currentSlot);
    }

    private void OnDisable()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            CancelInvoke(nameof(RandomizeAgentStats));
        }
    }
}


public enum AttackRange
{
    Close,
    Mid,
    Long
}