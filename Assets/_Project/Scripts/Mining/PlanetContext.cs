using System.Collections.Generic;

/*
 * PlanetContext
 * -------------
 * A read-only snapshot of planet state provided to probes during a tick.
 *
 * Purpose:
 * - Defines the information boundary between Planet and Probe
 * - Prevents probes from mutating live planet state
 * - Ensures deterministic, order-independent probe calculations
 *
 * Lifetime:
 * - Created once per tick
 * - Discarded after aggregation
 *
 * Rules:
 * - Contains data only
 * - No logic, no mutation
 */


public struct PlanetContext
{
    // -----------------------
    // Planet State
    // -----------------------
    public float currentReserves;
    public float maxReserves;

    // Precomputed so probes don't re-derive
    public float reserveEfficiency; // 0–1

    public int planetTier;

    // -----------------------
    // Probe Awareness
    // -----------------------
    public IReadOnlyList<Probe> probesOnPlanet;
}
