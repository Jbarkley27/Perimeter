/*
 * HeavyMiningProbe
 * ----------------
 * A high-output, high-cost late-game probe.
 *
 * Design intent:
 * - Extremely strong on high-tier planets
 * - Generates both Glass and Cores
 * - Best-in-slot for optimized builds
 */

public class HeavyMiningProbe : Probe
{
    public float coresPerGlass = 0.04f;

    public HeavyMiningProbe()
    {
        Type = ProbeType.HeavyMining;

        baseGlassRate = 8f;
        glassPerLevel = 3f;
    }

    protected override float GetGlassOutput(PlanetContext context)
    {
        float tierBonus = 1f + (0.1f * context.planetTier);
        float raw = baseGlassRate + (glassPerLevel * Level);
        return raw * tierBonus * context.reserveEfficiency;
    }

    protected override float GetCoreOutput(PlanetContext context)
    {
        return GetGlassOutput(context) * coresPerGlass;
    }
}
