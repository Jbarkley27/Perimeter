/*
 * DeepCoreProbe
 * -------------
 * A long-term utility probe that increases a planet's effective Glass reserves.
 *
 * Design intent:
 * - No direct resource generation
 * - Improves sustainability on high-output planets
 * - Best used on late-game or heavily mined planets
 */

public class DeepCoreProbe : Probe
{
    // Percentage increase to effective reserves per level
    public float reserveBonusPerLevel = 0.15f;

    public DeepCoreProbe()
    {
        Type = ProbeType.DeepCore;
    }

    public float GetReserveMultiplier()
    {
        return 1f + (reserveBonusPerLevel * Level);
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
