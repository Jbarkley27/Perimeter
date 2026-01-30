/*
 * ExtractorProbe
 * --------------
 * The baseline Glass-generating probe.
 *
 * Design intent:
 * - Primary early-game income source
 * - Scales linearly with level
 * - Affected by planet reserve efficiency
 * - Simple, reliable, and always useful
 */

public class ExtractorProbe : Probe
{
    public ExtractorProbe()
    {
        Type = ProbeType.Extractor;

        baseGlassRate = 5f;
        glassPerLevel = 1.5f;
    }

    protected override float GetGlassOutput(PlanetContext context)
    {
        float raw = baseGlassRate + (glassPerLevel * Level);
        return raw * context.reserveEfficiency;
    }
}
