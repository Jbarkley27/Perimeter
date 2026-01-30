/*
 * SurveyProbe
 * -----------
 * A progression utility probe focused on expansion and cost reduction.
 *
 * Design intent:
 * - Minimal resource generation
 * - Reduces planet upgrade or unlock costs
 * - Encourages exploration and prestige planning
 */

public class SurveyProbe : Probe
{
    public SurveyProbe()
    {
        Type = ProbeType.Survey;

        baseGlassRate = 1f;
        glassPerLevel = 0.25f;
    }

    protected override float GetGlassOutput(PlanetContext context)
    {
        float raw = baseGlassRate + (glassPerLevel * Level);
        return raw * context.reserveEfficiency;
    }
}
