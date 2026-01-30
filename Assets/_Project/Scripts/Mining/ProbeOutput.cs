/*
 * ProbeOutput
 * -----------
 * A value object representing the resource output of a probe for a single tick.
 *
 * Purpose:
 * - Decouples probe math from planet application
 * - Allows clean aggregation and modification
 *
 * Rules:
 * - Data only
 * - No logic
 * - No references to probes or planets
 */


public struct ProbeOutput
{
    public float glass;
    public float cores;

    public static readonly ProbeOutput Zero = new ProbeOutput
    {
        glass = 0f,
        cores = 0f
    };
}
