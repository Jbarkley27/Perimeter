/*
 * StabilizerProbe
 * ---------------
 * A consistency-focused utility probe that mitigates penalties from low reserves.
 *
 * Design intent:
 * - Reduces efficiency loss as reserves deplete
 * - Smooths income curves
 * - Improves idle reliability
 */

public class StabilizerProbe : Probe
{
    // Minimum efficiency floor increase per level
    public float efficiencyFloorBonus = 0.1f;

    public StabilizerProbe()
    {
        Type = ProbeType.Stabilizer;
    }

    public float GetEfficiencyFloor()
    {
        return efficiencyFloorBonus * Level;
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
