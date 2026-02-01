/*
 * RefineryProbe
 * -------------
 * A hybrid probe that generates Glass and converts a portion of it into Cores.
 *
 * Design intent:
 * - Primary meta-progression driver
 * - Slower Glass generation than Extractor
 * - Core output is derived from Glass processed
 * - Encourages long-term planning
 */

public class RefineryProbe : Probe
{
    // Cores generated per unit of Glass
    public float coresPerGlass = 0.02f;

    public RefineryProbe()
    {
        Type = ProbeType.Refinery;

        baseGlassRate = 2.5f;
        glassPerLevel = 0.75f;
    }


    protected override float GetGlassOutput(PlanetContext context)
    {
        float raw = baseGlassRate + (glassPerLevel * Level);
        return raw * context.reserveEfficiency;
    }

    

    protected override float GetCoreOutput(PlanetContext context)
    {
        float glass = GetGlassOutput(context);
        return glass * coresPerGlass;
    }
}
