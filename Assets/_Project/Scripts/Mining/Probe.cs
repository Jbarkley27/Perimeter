/*
 * Probe
 * -----
 * Abstract base class for all probe types.
 *
 * Purpose:
 * - Encapsulates probe-specific math only
 * - Produces resource output based on PlanetContext
 *
 * Design constraints:
 * - Probes are passive (no ticking, no state over time)
 * - Probes never mutate planet state
 * - Probes never reference UI, managers, or other probes directly
 *
 * Extension model:
 * - Each probe type inherits from Probe
 * - Override only the output methods you need
 */


public abstract class Probe
{
    public ProbeType Type { get; protected set; }
    public int Level { get; protected set; }

    // -----------------------
    // Base Stats (Data-Driven)
    // -----------------------
    public float baseGlassRate;
    public float baseCoreRate;

    public float glassPerLevel;
    public float corePerLevel;

    protected Probe()
    {
        Level = 0;
    }


    // -----------------------
    // Output
    // -----------------------
    public virtual ProbeOutput GetOutput(PlanetContext context)
    {
        return new ProbeOutput
        {
            glass = GetGlassOutput(context),
            cores = GetCoreOutput(context)
        };
    }

    protected virtual float GetGlassOutput(PlanetContext context)
    {
        return 0f;
    }

    protected virtual float GetCoreOutput(PlanetContext context)
    {
        return 0f;
    }

    // -----------------------
    // Progression
    // -----------------------
    public void Upgrade()
    {
        Level++;
        MiningManager.Instance.miningUI.RefreshPlanetUI(MiningManager.Instance.CurrentPlanet);
    }

    
}


/*
 * ProbeType
 * ---------
 * Enumerates all probe categories available in the game.
 *
 * Purpose:
 * - Identification
 * - UI labeling and filtering
 * - Save/load and serialization
 *
 * Rules:
 * - No logic
 * - Do not use for behavior branching
 */

public enum ProbeType
{
    Extractor,
    Refinery,
    DeepCore,
    Amplifier,
    Survey,
    Stabilizer,
    HeavyMining
}
