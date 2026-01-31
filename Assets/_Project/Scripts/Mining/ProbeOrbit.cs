using DG.Tweening;
using UnityEngine;

// Handles orbiting movement for a probe around a planet.
public class ProbeOrbit : MonoBehaviour
{
    public Transform center;
    public float speed = 20f;
    public bool clockwise = true;

    public Vector3 offset;
    private float angle;
    public float baseXAngle = 90f;
    private GameObject probeObject;
    private float tilt;

    void Start()
    {
        probeObject = null;
    }

    // Initializes the orbit with a fixed radius based on the spawn position.
    public void Init(Transform target, float orbitSpeed, bool isClockwise, GameObject probe)
    {
        center = target;
        speed = orbitSpeed;
        clockwise = isClockwise;
        probe.transform.position += offset;
        probeObject = probe;

        // change rotation to a random x angle to avoid uniformity
        angle = Random.Range(0f, 360f);
        tilt = Random.Range(-180, 180); // optional
        // Initial rotation
        // transform.rotation = Quaternion.Euler(new Vector3(angle, 0 , 0));
    }

    // Updates orbit position and rotates the probe to face the planet.
    private void Update()
    {
        if (!center) return;
        FacePlanet();
        OrbitPlanet();
    }

    public void FacePlanet()
    {
        if (!center || !probeObject) return;

        Vector3 toCenter = (center.position - probeObject.transform.position).normalized;
        probeObject.transform.rotation = Quaternion.LookRotation(toCenter, Vector3.up);
    }

    public void OrbitPlanet()
    {
        if (!center || !probeObject) return;

        float dir = clockwise ? -1f : 1f;
        angle += dir * speed * Time.deltaTime;
        
        transform.rotation =
            Quaternion.AngleAxis(tilt, Vector3.right) *
            Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
