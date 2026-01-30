using UnityEngine;

public class ProbeVisual : MonoBehaviour
{
    public ParticleSystem particles;
    public float particlesPerRate = 5f;
    public int minParticles = 5;
    public int maxParticles = 200;

    public void SetOutput(ProbeOutput output)
    {
        float rate = output.glass + output.cores;
        int count = Mathf.Clamp(Mathf.RoundToInt(rate * particlesPerRate), minParticles, maxParticles);

        var main = particles.main;
        main.maxParticles = count;

        var emission = particles.emission;
        emission.rateOverTime = count;
    }
}
