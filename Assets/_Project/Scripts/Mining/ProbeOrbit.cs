using UnityEngine;

// Handles orbiting movement for a probe around a planet.
public class ProbeOrbit : MonoBehaviour
{
    public Transform center;
    public float speed = 20f;
    public bool clockwise = true;

    private Vector3 offset;
    private float angle;

    // Initializes the orbit with a fixed radius based on the spawn position.
    public void Init(Transform target, Vector3 startWorldPosition, float orbitSpeed, bool isClockwise)
    {
        center = target;
        speed = orbitSpeed;
        clockwise = isClockwise;
        offset = startWorldPosition - center.position;

        Debug.Log($"[ProbeOrbit] Init offset magnitude: {offset.magnitude}");
    }

    // Updates orbit position and rotates the probe to face the planet.
    private void Update()
    {
        if (!center) return;

        Debug.Log("Orbiting around center at position: " + center.position);

        float dir = clockwise ? -1f : 1f;
        angle += dir * speed * Time.deltaTime;

        // Vector3 rotated = Quaternion.AngleAxis(angle, Vector3.up) * offset;
        // transform.position = center.position + rotated;

        // Always face the planet
        // Vector3 toCenter = (center.position - transform.position).normalized;
        // transform.rotation = Quaternion.LookRotation(toCenter, Vector3.up);
        FacePlanet();
    }

    public void FacePlanet()
    {
        if (!center) return;

        Vector3 toCenter = (center.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(toCenter, Vector3.up);
    }
}
