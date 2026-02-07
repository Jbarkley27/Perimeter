using UnityEngine;

/*
 * UIRectRotation
 * --------------
 * Rotates a UI RectTransform at a constant speed (degrees per second).
 * Intended for subtle ambient motion on UI elements.
 */
public class UIRectRotation : MonoBehaviour
{
    [Header("Target")]
    public RectTransform target;

    [Header("Rotation Settings")]
    public float degreesPerSecond = 10f;
    public bool useUnscaledTime = true;
    public bool rotateClockwise = true;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (target == null)
            return;

        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float direction = rotateClockwise ? -1f : 1f;
        float deltaAngle = degreesPerSecond * direction * delta;

        target.Rotate(0f, 0f, deltaAngle);
    }
}
