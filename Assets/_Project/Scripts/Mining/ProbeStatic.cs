using UnityEngine;

// Holds the visual anchor for stationary probe spawning.
public class ProbeStatic : MonoBehaviour
{
    public Transform particleEffect;

    void Awake()
    {
        if (particleEffect == null)
            particleEffect = transform.GetChild(0);


        if (particleEffect != null)
            particleEffect.gameObject.SetActive(false);
            
    }

    // Returns the transform used for spawning the stationary probe visual.
    public void ActiveParticleEffect()
    {
        if (particleEffect != null)
            particleEffect.gameObject.SetActive(true);
    }
}
