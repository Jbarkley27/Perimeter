/*
 * AmplifierProbe
 * ---------------
 * A passive utility probe that increases the effectiveness of other probes
 * deployed on the same planet.
 *
 * Design intent:
 * - Does not generate Glass or Cores directly
 * - Encourages probe composition decisions
 * - Scaling is predictable and transparent
 * - All math is applied by Planet, not the probe itself
 */

public class AmplifierProbe : Probe
{
    // Percentage bonus per level (e.g. 0.1 = +10%)
    public float bonusPerLevel = 0.1f;

    public AmplifierProbe()
    {
        Type = ProbeType.Amplifier;
    }

    public float GetMultiplier(PlanetContext context)
    {
        return 1f + (bonusPerLevel * Level);
    }

    protected override float GetGlassOutput(PlanetContext context)
    {
        return 0f;
    }

    protected override float GetCoreOutput(PlanetContext context)
    {
        return 0f;
    }
}
