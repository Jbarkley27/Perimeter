using UnityEngine;

// Holds the visual anchor for stationary probe spawning.
public class ProbeStatic : MonoBehaviour
{
    public Transform visualAnchor;

    // Returns the transform used for spawning the stationary probe visual.
    public Transform GetSpawnTransform()
    {
        return visualAnchor != null ? visualAnchor : transform;
    }
}
