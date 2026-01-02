using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    private Vector3 travelDirection;
    private Transform target;
    private Vector3 lastTargetPosition;
    private Vector3 targetVelocity;

    private float speed;
    private float baseSpeed;
    private float damage;

    [Header("Accuracy / Spread")]
    public float accuracyAngle = 45f;
    public float lifetime = 5f;

    [Header("Guidance Timing")]
    public float guidanceDelay = 0.4f;
    public float preGuidanceSpeedMultiplier = 0.5f;
    public float speedRampRate = 2f;

    [Header("Adaptive Turn Rate")]
    public float minTurnRate = 2f;
    public float maxTurnRate = 14f;
    public float turnRampDuration = 1.5f;

    [Header("Intercept Guidance")]
    public float maxLeadTime = 0.6f;
    public float leadDistanceScale = 10f;

    private float guidanceTimer;
    private bool isGuiding;
    private float flightTime;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // -------------------------------------------------------
    // LAUNCH
    // -------------------------------------------------------
    public void Launch(
        Transform targetTransform,
        float dmg,
        float projectileSpeed,
        float accuracyDegrees
    )
    {
        target = targetTransform;
        damage = dmg;

        baseSpeed = projectileSpeed;
        speed = projectileSpeed * preGuidanceSpeedMultiplier;
        accuracyAngle = accuracyDegrees;

        guidanceTimer = guidanceDelay;
        isGuiding = false;
        flightTime = 0f;

        if (target != null)
            lastTargetPosition = target.position;

        Vector3 idealDirection = (target.position - transform.position).normalized;
        travelDirection = ApplyAccuracySpread(idealDirection, accuracyAngle);

        transform.rotation = Quaternion.LookRotation(travelDirection);

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(travelDirection * speed, ForceMode.VelocityChange);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        HandleGuidanceTimer();
    }

    private void FixedUpdate()
    {
        flightTime += Time.fixedDeltaTime;
        UpdateTargetVelocity();
        HandleMovement();
    }

    // -------------------------------------------------------
    // GUIDANCE STATE
    // -------------------------------------------------------
    private void HandleGuidanceTimer()
    {
        if (isGuiding)
            return;

        guidanceTimer -= Time.deltaTime;
        if (guidanceTimer <= 0f)
            isGuiding = true;
    }

    // -------------------------------------------------------
    // TARGET VELOCITY ESTIMATION
    // -------------------------------------------------------
    private void UpdateTargetVelocity()
    {
        if (target == null)
            return;

        targetVelocity = (target.position - lastTargetPosition) / Time.fixedDeltaTime;
        lastTargetPosition = target.position;
    }

    // -------------------------------------------------------
    // MOVEMENT AND GUIDANCE
    // -------------------------------------------------------
    private void HandleMovement()
    {
        if (isGuiding && target != null)
        {
            Vector3 toTarget = target.position - transform.position;
            float distance = toTarget.magnitude;

            float leadFactor = Mathf.Clamp01(distance / leadDistanceScale);
            float leadTime = Mathf.Lerp(0f, maxLeadTime, leadFactor);

            Vector3 interceptPoint = target.position + targetVelocity * leadTime;
            Vector3 desiredDirection = (interceptPoint - transform.position).normalized;

            float turnT = Mathf.Clamp01(flightTime / turnRampDuration);
            float currentTurnRate = Mathf.Lerp(minTurnRate, maxTurnRate, turnT);

            travelDirection = Vector3.Slerp(
                travelDirection,
                desiredDirection,
                currentTurnRate * Time.fixedDeltaTime
            ).normalized;

            transform.rotation = Quaternion.LookRotation(travelDirection);

            speed = Mathf.Lerp(speed, baseSpeed, speedRampRate * Time.fixedDeltaTime);
            rb.linearVelocity = travelDirection * speed;
        }
    }

    // -------------------------------------------------------
    // COLLISION
    // -------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    // -------------------------------------------------------
    // ACCURACY CONE
    // -------------------------------------------------------
    private Vector3 ApplyAccuracySpread(Vector3 forward, float maxAngle)
    {
        if (maxAngle <= 0f)
            return forward;

        float maxRadians = maxAngle * Mathf.Deg2Rad;

        Vector3 randomOffset = Random.insideUnitSphere;
        randomOffset -= Vector3.Dot(randomOffset, forward) * forward;
        randomOffset.Normalize();

        float spreadRadius = Mathf.Tan(maxRadians);
        return (forward + randomOffset * spreadRadius).normalized;
    }
}
