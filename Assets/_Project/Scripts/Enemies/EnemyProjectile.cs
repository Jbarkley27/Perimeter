using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody rb;
    private int damage;

    [SerializeField]
    private float lifetime = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Launch(
        Transform target,
        int dmg,
        float speed
    )
    {
        damage = dmg;

        Vector3 direction = (target.position - transform.position).normalized;

        transform.rotation = Quaternion.LookRotation(direction);
        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Draw a debug line to show the projectile's path
        Debug.DrawRay(transform.position, rb.linearVelocity.normalized * 2f, Color.red);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"EnemyProjectile collided with {collision.gameObject.name}");
        if (!collision.gameObject.CompareTag("PlayerHit"))
            return;

        GlobalDataStore.Instance.BarrierModule.TakeDamage(damage);

        Destroy(gameObject);
    }
}
