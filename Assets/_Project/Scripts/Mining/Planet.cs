using System.Collections.Generic;
using UnityEngine;

/*
 * Planet
 * ------
 * Represents a single resource-bearing planet that can host probes.
 *
 * Responsibilities:
 * - Owns probe instances deployed on this planet
 * - Tracks Glass reserves and depletion
 * - Aggregates probe outputs once per tick
 * - Applies planet-level modifiers (Amplifiers, Stabilizers, Deep Core)
 *
 * Design constraints:
 * - Planet controls ALL state mutation
 * - Probes are passive and stateless
 * - No UI logic
 * - No economy or meta-progression logic
 *
 * Data Flow:
 * Planet -> PlanetContext -> Probes -> ProbeOutput -> Planet
 */


public class Planet : MonoBehaviour
{
    [Header("Identity")]
    public string planetName;
    public bool Locked = true;
    public int planetTier;

    [Header("Reserves")]
    public float currentReserves;
    public float maxReserves;

    [Header("Probes")]
    [SerializeField]
    private List<Probe> probes = new List<Probe>();

    public IReadOnlyList<Probe> Probes => probes;
    public int MaxProbesAllowed = 2; // 2 by default, can be modified via upgrades


    public List<Transform> stationarySlots;
    public List<Transform> orbitSlots;

    private bool[] stationaryOccupied;
    private bool[] orbitOccupied;


    void Awake()
    {
        stationaryOccupied = new bool[stationarySlots.Count];
        orbitOccupied = new bool[orbitSlots.Count];
    }


    // -----------------------
    // TICK
    // -----------------------
    public void Tick(float deltaTime)
    {
        // No probes = nothing to do
        if (probes.Count == 0)
            return;

        // Build immutable context snapshot
        PlanetContext context = BuildContext();

        float totalGlass = 0f;
        float totalCores = 0f;

        // Calculate amplifier multiplier once per tick
        float amplifierMultiplier = CalculateAmplifierMultiplier(context);

        // Collect output from each probe
        foreach (Probe probe in probes)
        {
            ProbeOutput output = probe.GetOutput(context);
            ProbeManager.Instance.UpdateProbeVisual(probe, output);
            totalGlass += output.glass;
            totalCores += output.cores;
        }

        // Apply amplifier scaling after aggregation
        totalGlass *= amplifierMultiplier;
        totalCores *= amplifierMultiplier;

        // Apply resource changes scaled by time
        ApplyGlass(totalGlass * deltaTime);
        ApplyCores(totalCores * deltaTime);
    }




    // Builds a read-only snapshot of the planet state for this tick.
    // This context is passed to probes so they can calculate output
    // without mutating live planet data.
    public PlanetContext BuildContext()
    {
        // Step 1: Calculate effective max reserves
        // Deep Core probes increase reserve capacity but do not refill reserves
        float effectiveMaxReserves = GetEffectiveMaxReserves();

        // Step 2: Calculate raw efficiency based on current reserves
        // Efficiency is a normalized 0–1 value
        float efficiency = CalculateReserveEfficiency(effectiveMaxReserves);

        // Step 3: Apply Stabilizer probes
        // Stabilizers raise the minimum efficiency floor to prevent collapse
        efficiency = ApplyStabilizerFloor(efficiency);

        // Step 4: Build and return the immutable context
        return new PlanetContext
        {
            currentReserves = currentReserves,
            maxReserves = effectiveMaxReserves,
            reserveEfficiency = efficiency,
            planetTier = planetTier,
            probesOnPlanet = Probes
        };
    }


    // Calculates how efficient probes are based on remaining reserves.
    // This does NOT account for stabilizers — that is applied separately.
    private float CalculateReserveEfficiency(float effectiveMaxReserves)
    {
        // Safety check to avoid divide-by-zero
        if (effectiveMaxReserves <= 0f)
            return 0f;

        // Ratio of remaining reserves to maximum reserves
        float ratio = currentReserves / effectiveMaxReserves;

        // Clamp to ensure a valid 0–1 range
        return Mathf.Clamp01(ratio);
    }


    // Calculates the effective maximum reserves for this planet.
    // Deep Core probes increase reserve capacity multiplicatively.
    private float GetEffectiveMaxReserves()
    {
        float multiplier = 1f;

        // Each Deep Core probe contributes a reserve multiplier
        foreach (Probe probe in probes)
        {
            if (probe is DeepCoreProbe deepCore)
            {
                multiplier *= deepCore.GetReserveMultiplier();
            }
        }

        // Effective max reserves are scaled, but current reserves are unchanged
        return maxReserves * multiplier;
    }

    // Applies a minimum efficiency floor based on Stabilizer probes.
    // This prevents probe output from collapsing too hard at low reserves.
    private float ApplyStabilizerFloor(float efficiency)
    {
        float floor = 0f;

        // Each Stabilizer contributes to the minimum efficiency floor
        foreach (Probe probe in probes)
        {
            if (probe is StabilizerProbe stabilizer)
            {
                floor += stabilizer.GetEfficiencyFloor();
            }
        }

        // Final efficiency is the higher of:
        // - calculated reserve efficiency
        // - stabilizer-defined floor
        return Mathf.Clamp01(Mathf.Max(efficiency, floor));
    }

    // Calculates a final output multiplier from Amplifier probes.
    // This multiplier is applied after all probe outputs are summed.
    private float CalculateAmplifierMultiplier(PlanetContext context)
    {
        float multiplier = 1f;

        // Each Amplifier probe multiplies total output
        foreach (Probe probe in probes)
        {
            if (probe is AmplifierProbe amplifier)
            {
                multiplier *= amplifier.GetMultiplier(context);
            }
        }

        return multiplier;
    }





    // -----------------------
    // PROBE MANAGEMENT
    // -----------------------
    public bool CanAddProbe()
    {
        if (probes.Count >= MaxProbesAllowed)
            return false;
        return true;
    }

    public void AddProbe(Probe probe)
    {
        if (!CanAddProbe())
            return;

        probes.Add(probe);
    }

    public void RemoveProbe(Probe probe)
    {
        probes.Remove(probe);
    }


    // -----------------------
    // APPLY OUTPUTS
    // -----------------------
    private void ApplyGlass(float amount)
    {
        if (amount <= 0f)
            return;

        currentReserves = Mathf.Max(0f, currentReserves - amount);

        if (GlassManager.Instance != null)
            GlassManager.Instance.AddGlass(amount);
    }


    private void ApplyCores(float amount)
    {
        if (amount <= 0f)
            return;

        if (CoreManager.Instance != null)
            CoreManager.Instance.AddCores(amount);
    }



    public bool TryGetStationarySlot(out Transform slot, out int index)
    {
        for (int i = 0; i < stationarySlots.Count; i++)
        {
            if (!stationaryOccupied[i])
            {
                stationaryOccupied[i] = true;
                slot = stationarySlots[i];
                index = i;
                return true;
            }
        }
        slot = null;
        index = -1;
        return false;
    }

    public bool TryGetOrbitSlot(out Transform slot, out int index)
    {
        for (int i = 0; i < orbitSlots.Count; i++)
        {
            if (!orbitOccupied[i])
            {
                orbitOccupied[i] = true;
                slot = orbitSlots[i];
                index = i;
                return true;
            }
        }
        slot = null;
        index = -1;
        return false;
    }

}
