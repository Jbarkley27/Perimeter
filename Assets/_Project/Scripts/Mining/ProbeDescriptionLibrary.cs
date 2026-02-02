/*
 * ProbeDescriptionLibrary
 * -----------------------
 * Centralized, player-facing descriptions for probes.
 */
public static class ProbeDescriptionLibrary
{
    
    // Returns player-facing description by probe type.
    public static string GetDescription(ProbeType type)
    {
        switch (type)
        {
            case ProbeType.Extractor:
                return "Baseline Glass mining. Scales with reserves.";
            case ProbeType.Refinery:
                return "Mines Glass and converts a portion into Cores.";
            case ProbeType.DeepCore:
                return "Increases max reserves, improving long-term yield.";
            case ProbeType.Amplifier:
                return "Boosts the output of other probes on this planet.";
            case ProbeType.Survey:
                return "Light Glass output and expansion utility.";
            case ProbeType.Stabilizer:
                return "Raises minimum efficiency as reserves decline.";
            case ProbeType.HeavyMining:
                return "High-output mining, strong on high-tier planets.";
            default:
                return "";
        }
    }


    public static string GetDescription(Probe probe)
    {
        if (probe == null)
            return string.Empty;

        return GetDescription(probe.Type);
    }

}
