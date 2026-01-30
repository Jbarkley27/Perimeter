using UnityEngine;

public class ProbeOrbit : MonoBehaviour
{
    public Transform center;
    public float speed = 20f;
    public bool clockwise = true;

    public void Init(Transform target, float orbitSpeed, bool isClockwise)
    {
        center = target;
        speed = orbitSpeed;
        clockwise = isClockwise;
    }

    void Update()
    {
        if (!center) return;
        float dir = clockwise ? -1f : 1f;
        transform.RotateAround(center.position, Vector3.up, dir * speed * Time.deltaTime);
    }
}
