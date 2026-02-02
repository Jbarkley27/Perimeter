using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public List<Probe> probes = new List<Probe>();

    public IReadOnlyList<Probe> Probes => probes;
    public int MaxProbeSlots = 2; // 2 by default, can be modified via upgrades


    public List<Transform> stationarySlots;
    public List<Transform> orbitSlots;

    private bool[] stationaryOccupied;
    private bool[] orbitOccupied;

    [Header("Planet Upgrades")]
    [SerializeField] private int maxStartingUpgrades = PlanetUpgradeTuning.DefaultMaxStartingUpgrades;
    [SerializeField] private List<PlanetUpgradeInstance> upgrades = new List<PlanetUpgradeInstance>();

    // Runtime-only set for upgrades that should never appear again after being rolled.
    // Not serialized, so it resets on a fresh session (fine for now).
    private readonly HashSet<PlanetUpgradeId> removedFromRollPool = new HashSet<PlanetUpgradeId>();

    // Tracks how long each probe has been deployed on this planet (seconds).
    private readonly Dictionary<Probe, float> probeDeployedSeconds = new Dictionary<Probe, float>();

    public IReadOnlyList<PlanetUpgradeInstance> Upgrades => upgrades;
    public int MaxStartingUpgrades => maxStartingUpgrades;


    // Tracks how much Glass has been spent on upgrades (for refund on reroll).
    private double glassSpentOnUpgrades = 0;

    // Predictive Logistics free probe usage flag (reset on reroll).
    private bool predictiveLogisticsFreeProbeUsed = false;

    // Tracks Glass spent per upgrade (used for individual refunds).
    private readonly Dictionary<PlanetUpgradeId, double> upgradeSpendById = new Dictionary<PlanetUpgradeId, double>();





    void Awake()
    {
        stationaryOccupied = new bool[stationarySlots.Count];
        orbitOccupied = new bool[orbitSlots.Count];
        currentReserves = maxReserves;
    }


    // Initializes upgrades the first time the planet is created.
    private void Start()
    {
        EnsureStartingUpgrades();
    }



        // Returns the current level of an upgrade on this planet (0 if missing).
    public int GetUpgradeLevel(PlanetUpgradeId id)
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i].id == id)
                return upgrades[i].level;
        }
        return 0;
    }

    // Returns true if the planet has this upgrade at any level.
    public bool HasUpgrade(PlanetUpgradeId id)
    {
        return GetUpgradeLevel(id) > 0;
    }

    // Returns true if this upgrade is at its maximum level for this planet.
    public bool IsUpgradeMaxed(PlanetUpgradeId id)
    {
        PlanetUpgradeDefinition def = PlanetUpgradeCatalog.Get(id);
        if (def == null)
            return true;

        return GetUpgradeLevel(id) >= def.maxLevel;
    }

    // Replaces the current upgrade list (used for rolling/rerolling).
    public void SetUpgrades(List<PlanetUpgradeInstance> newUpgrades)
    {
        upgrades.Clear();
        upgrades.AddRange(newUpgrades);

        // Ensure one-time upgrades (like Predictive Logistics) never appear again.
        for (int i = 0; i < upgrades.Count; i++)
            MarkUpgradeRolled(upgrades[i].id);
    }


    // Marks an upgrade as rolled so it can be removed from future rolls.
    // Predictive Logistics can optionally re-enter the pool via MiningManager setting.
    public void MarkUpgradeRolled(PlanetUpgradeId id)
    {
        PlanetUpgradeDefinition def = PlanetUpgradeCatalog.Get(id);
        if (def == null || !def.removeFromPoolAfterRoll)
            return;

        bool allowRepeat = MiningManager.Instance != null && MiningManager.Instance.allowPredictiveLogisticsRepeatRolls;
        if (id == PlanetUpgradeId.PredictiveLogistics && allowRepeat)
            return;

        removedFromRollPool.Add(id);
    }


    // Returns true if this upgrade is blocked from future rolls on this planet.
    public bool IsUpgradeRemovedFromPool(PlanetUpgradeId id)
    {
        return removedFromRollPool.Contains(id);
    }

    // Returns how long a probe has been deployed on this planet (in seconds).
    public float GetProbeDeployedSeconds(Probe probe)
    {
        if (probe == null)
            return 0f;

        if (probeDeployedSeconds.TryGetValue(probe, out float seconds))
            return seconds;

        return 0f;
    }

    // Resets the timer when a probe is deployed on this planet.
    public void RegisterProbeDeployment(Probe probe)
    {
        if (probe == null)
            return;

        probeDeployedSeconds[probe] = 0f;
    }

    // Removes tracking when a probe leaves this planet.
    public void UnregisterProbeDeployment(Probe probe)
    {
        if (probe == null)
            return;

        probeDeployedSeconds.Remove(probe);
    }


        // Ensures a planet always starts with a full set of upgrades.
    public void EnsureStartingUpgrades()
    {
        if (upgrades.Count > 0)
        {
            // If upgrades already exist, respect them and register one-time removals.
            for (int i = 0; i < upgrades.Count; i++)
                MarkUpgradeRolled(upgrades[i].id);
            return;
        }

        RollUpgrades(maxStartingUpgrades);
    }

    // Attempts to reroll upgrades (costs Glass, refunds previous upgrade spend).
    public bool TryRerollUpgrades()
    {
        if (GlassManager.Instance == null)
            return false;

        if (!GlassManager.Instance.SpendGlass(PlanetUpgradeTuning.RerollCost))
            return false;

        RefundUpgradeSpending();
        RollUpgrades(maxStartingUpgrades);

        return true;
    }

        // Returns the max probe level on this planet (base + Scalable Framework).
    public int GetMaxProbeLevel()
    {
        int bonus = GetUpgradeLevel(PlanetUpgradeId.ScalableFramework);
        return PlanetUpgradeTuning.BaseProbeMaxLevel + bonus;
    }

    // Returns the probe upgrade cost reduction from Automated Calibration.
    public float GetProbeUpgradeCostReduction()
    {
        int level = GetUpgradeLevel(PlanetUpgradeId.AutomatedCalibration);
        return PlanetUpgradeTuning.GetUpgradeCostReduction(level);
    }

    // Returns the per-probe buy discount from Mass Fabrication.
    public float GetProbeBuyCostDiscountPerProbe()
    {
        int level = GetUpgradeLevel(PlanetUpgradeId.MassFabrication);
        return PlanetUpgradeTuning.GetMassFabricationDiscountPerProbe(level);
    }

    // Returns true if Predictive Logistics can grant a free probe.
    public bool CanUsePredictiveLogisticsFreeProbe()
    {
        return HasUpgrade(PlanetUpgradeId.PredictiveLogistics) && !predictiveLogisticsFreeProbeUsed;
    }

    // Consumes the Predictive Logistics free probe.
    public void UsePredictiveLogisticsFreeProbe()
    {
        predictiveLogisticsFreeProbeUsed = true;
    }

    // Applies reserve regeneration from Subsurface Vents.
    private void ApplyReserveRegen(float effectiveMaxReserves, float deltaTime)
    {
        int level = GetUpgradeLevel(PlanetUpgradeId.SubsurfaceVents);
        if (level <= 0)
            return;

        if (effectiveMaxReserves <= 0f)
            return;

        // Regen as a % of max reserves per second.
        float regenPerSecond = PlanetUpgradeTuning.ReserveRegenPerSecondPerLevel * level;
        float regenAmount = effectiveMaxReserves * regenPerSecond * deltaTime;

        currentReserves = Mathf.Min(effectiveMaxReserves, currentReserves + regenAmount);
    }

    // Applies planet-level modifiers to probe output before amplification.
    private ProbeOutput ApplyPlanetOutputModifiers(Probe probe, ProbeOutput output)
    {
        output = ApplyLongTermYield(probe, output);
        output = ApplyRefinedOutput(probe, output);
        return output;
    }

    // Long-Term Yield increases output based on time deployed.
    private ProbeOutput ApplyLongTermYield(Probe probe, ProbeOutput output)
    {
        int level = GetUpgradeLevel(PlanetUpgradeId.LongTermYield);
        if (level <= 0)
            return output;

        float minutes = GetProbeDeployedSeconds(probe) / 60f;
        float bonus = PlanetUpgradeTuning.GetLongTermYieldBonus(minutes, level);
        float multiplier = 1f + bonus;

        output.glass *= multiplier;
        output.cores *= multiplier;
        return output;
    }

    // Refined Output boosts core generation for Refinery probes only.
    private ProbeOutput ApplyRefinedOutput(Probe probe, ProbeOutput output)
    {
        int level = GetUpgradeLevel(PlanetUpgradeId.RefinedOutput);
        if (level <= 0)
            return output;

        if (probe is RefineryProbe)
        {
            float multiplier = 1f + (PlanetUpgradeTuning.RefinedOutputBonusPerLevel * level);
            output.cores *= multiplier;
        }

        return output;
    }


    // Rolls a new set of upgrades with no duplicates.
    private void RollUpgrades(int count)
    {
        Debug.Log("Re-rolling");
        upgradeSpendById.Clear();

        upgrades.Clear();
        glassSpentOnUpgrades = 0;
        // Only reset Predictive Logistics usage if repeat rolls are allowed.
        if (MiningManager.Instance != null && MiningManager.Instance.allowPredictiveLogisticsRepeatRolls)
            predictiveLogisticsFreeProbeUsed = false;


        List<PlanetUpgradeDefinition> pool = new List<PlanetUpgradeDefinition>();
        foreach (var def in PlanetUpgradeCatalog.AllDefinitions)
        {
            if (!IsUpgradeRemovedFromPool(def.id))
                pool.Add(def);
        }

        int rollCount = Mathf.Min(count, pool.Count);

        for (int i = 0; i < rollCount; i++)
        {
            PlanetUpgradeDefinition selected = SelectWeightedUpgrade(pool);
            if (selected == null)
                break;

            upgrades.Add(new PlanetUpgradeInstance
            {
                id = selected.id,
                level = 1
            });

            MarkUpgradeRolled(selected.id);
            pool.Remove(selected);
        }
    }

    // Weighted roll: higher weight = more likely.
    private PlanetUpgradeDefinition SelectWeightedUpgrade(List<PlanetUpgradeDefinition> pool)
    {
        float totalWeight = 0f;
        for (int i = 0; i < pool.Count; i++)
            totalWeight += pool[i].weight;

        if (totalWeight <= 0f)
            return null;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            cumulative += pool[i].weight;
            if (roll <= cumulative)
                return pool[i];
        }

        return pool[pool.Count - 1];
    }

    // Refunds all Glass spent on upgrades for this planet.
    private void RefundUpgradeSpending()
    {
        if (glassSpentOnUpgrades <= 0)
            return;

        if (GlassManager.Instance != null)
            GlassManager.Instance.AddGlass(glassSpentOnUpgrades);

        upgradeSpendById.Clear();


        glassSpentOnUpgrades = 0;
    }


    // Returns the Glass refund for resetting all upgrades (scaled by refund %).
    public double GetUpgradeRefundAmount()
    {
        float refundPercent = MiningManager.Instance != null
            ? MiningManager.Instance.planetUpgradeRefundPercent
            : 1f;

        return glassSpentOnUpgrades * refundPercent;
    }

    // Refunds all upgrade spending (at refund %) and resets upgrades to level 1.
    public bool TryRefundAllUpgrades()
    {
        if (glassSpentOnUpgrades <= 0)
            return false;

        double refundAmount = GetUpgradeRefundAmount();

        if (GlassManager.Instance != null && refundAmount > 0)
            GlassManager.Instance.AddGlass(refundAmount);

        // Reset to starting levels but keep the same upgrade set.
        for (int i = 0; i < upgrades.Count; i++)
            upgrades[i].level = 1;
        
        upgradeSpendById.Clear();


        glassSpentOnUpgrades = 0;

        return true;
    }



    // Returns refund amount for a single upgrade (scaled by refund %).
    public double GetUpgradeRefundAmount(PlanetUpgradeId id)
    {
        if (!upgradeSpendById.TryGetValue(id, out double spent))
            return 0;

        float refundPercent = MiningManager.Instance != null
            ? MiningManager.Instance.planetUpgradeRefundPercent
            : 1f;

        return spent * refundPercent;
    }

    // Refunds a single upgrade back to level 1.
    public bool TryRefundUpgrade(PlanetUpgradeId id)
    {
        PlanetUpgradeInstance instance = GetUpgradeInstance(id);
        if (instance == null || instance.level <= 1)
            return false;

        if (!upgradeSpendById.TryGetValue(id, out double spent) || spent <= 0)
            return false;

        double refundAmount = GetUpgradeRefundAmount(id);

        if (GlassManager.Instance != null && refundAmount > 0)
            GlassManager.Instance.AddGlass(refundAmount);

        // Reset this upgrade to level 1.
        instance.level = 1;

        // Remove spend from totals (so it isn't refunded again later).
        glassSpentOnUpgrades -= spent;
        upgradeSpendById[id] = 0;

        return true;
    }




    // -----------------------
    // TICK
    // -----------------------
        // Runs one mining tick for this planet.
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
            Debug.Assert(probe != null, "Null probe found on planet: " + planetName);
            if (probe == null)
                continue;

            // Track how long this probe has been deployed (used by Long-Term Yield).
            if (!probeDeployedSeconds.ContainsKey(probe))
                probeDeployedSeconds[probe] = 0f;

            probeDeployedSeconds[probe] += deltaTime;

            ProbeOutput output = probe.GetOutput(context);
            output = ApplyPlanetOutputModifiers(probe, output);

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

        // Regenerate reserves after mining (Subsurface Vents).
        ApplyReserveRegen(context.maxReserves, deltaTime);
    }


    // Aggregates probe output for UI display (per-second, before deltaTime).
    public ProbeOutput GetAggregatedOutput()
    {
        if (probes.Count == 0)
            return ProbeOutput.Zero;

        PlanetContext context = BuildContext();

        float totalGlass = 0f;
        float totalCores = 0f;

        foreach (Probe probe in probes)
        {
            if (probe == null)
                continue;

            ProbeOutput output = probe.GetOutput(context);
            output = ApplyPlanetOutputModifiers(probe, output);

            totalGlass += output.glass;
            totalCores += output.cores;
        }

        float amplifierMultiplier = CalculateAmplifierMultiplier(context);
        totalGlass *= amplifierMultiplier;
        totalCores *= amplifierMultiplier;

        return new ProbeOutput
        {
            glass = totalGlass,
            cores = totalCores
        };
    }



     // Returns true if the planet has no available probe slots (including upgrades).
    public bool MaxProbeSlotsReached()
    {
        return probes.Count >= GetEffectiveMaxProbeSlots();
    }

    // Returns total probe slot capacity including Orbital Expansion.
    public int GetEffectiveMaxProbeSlots()
    {
        int bonus = HasUpgrade(PlanetUpgradeId.OrbitalExpansion) ? 1 : 0;
        return MaxProbeSlots + bonus;
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



    // Provides a read-only snapshot of the planet state for UI calculations.
    public PlanetContext GetProbeContext()
    {
        return BuildContext();
    }


    public double GetCurrentReserveRatio()
    {
        if (maxReserves <= 0f)
            return 0.0;

        return (double)(currentReserves / maxReserves);
    }

    // Calculates how efficient probes are based on remaining reserves.
    // This does NOT account for stabilizers — that is applied separately.
    private float CalculateReserveEfficiency(float effectiveMaxReserves)
    {
        // Safety check to avoid divide-by-zero
        if (effectiveMaxReserves <= 0f)
            return 0f;

        // Ratio of remaining reserves to maximum reserves (0–1).
        float ratio = Mathf.Clamp01(currentReserves / effectiveMaxReserves);

        // Stratified Core makes the curve fall more slowly.
        // Lower exponent => slower decline as reserves deplete.
        float exponent = PlanetUpgradeTuning.GetEfficiencyExponent(GetUpgradeLevel(PlanetUpgradeId.StratifiedCore));
        float efficiency = Mathf.Pow(ratio, exponent);

        // Conversion Buffer: if reserves are empty, keep a small floor.
        if (currentReserves <= 0f)
        {
            float bufferFloor = PlanetUpgradeTuning.GetConversionBufferFloor(
                GetUpgradeLevel(PlanetUpgradeId.ConversionBuffer)
            );
            efficiency = Mathf.Max(efficiency, bufferFloor);
        }

        return Mathf.Clamp01(efficiency);
    }



    // Calculates the effective maximum reserves for this planet.
    // Reserve upgrades add a flat percentage, Deep Core multiplies after.
    private float GetEffectiveMaxReserves()
    {
        float reserveBonus = 0f;

        reserveBonus += PlanetUpgradeTuning.LargeReserveBonusPerLevel * GetUpgradeLevel(PlanetUpgradeId.LargeReserve);
        reserveBonus += PlanetUpgradeTuning.DeepMantleBonusPerLevel * GetUpgradeLevel(PlanetUpgradeId.DeepMantle);
        reserveBonus += PlanetUpgradeTuning.CrystallineMantleBonusPerLevel * GetUpgradeLevel(PlanetUpgradeId.CrystallineMantle);

        float multiplier = 1f + reserveBonus;

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


    // Applies a minimum efficiency floor based on Stabilizer probes and upgrades.
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

        // Hardened Crust adds an additional floor.
        floor += PlanetUpgradeTuning.EfficiencyFloorPerLevel * GetUpgradeLevel(PlanetUpgradeId.HardenedCrust);

        // Final efficiency is the higher of:
        // - calculated reserve efficiency
        // - stabilizer/upgrade-defined floor
        return Mathf.Clamp01(Mathf.Max(efficiency, floor));
    }


    // Calculates a final output multiplier from Amplifier probes.
    // Resonant Lattice strengthens amplifier bonuses.
    // Signal Relay adds a bonus amplifier-like stack.
    private float CalculateAmplifierMultiplier(PlanetContext context)
    {
        float multiplier = 1f;

        float latticeMultiplier = 1f + (PlanetUpgradeTuning.ResonantLatticeBonusPerLevel
            * GetUpgradeLevel(PlanetUpgradeId.ResonantLattice));

        // Each Amplifier probe multiplies total output
        foreach (Probe probe in probes)
        {
            if (probe is AmplifierProbe amplifier)
            {
                // Convert to bonus, strengthen it, then reapply.
                float ampBonus = amplifier.GetMultiplier(context) - 1f;
                ampBonus *= latticeMultiplier;
                multiplier *= 1f + ampBonus;
            }
        }

        // Signal Relay adds a separate amplifier-style bonus.
        float relayBonus = PlanetUpgradeTuning.SignalRelayBonusPerLevel
            * GetUpgradeLevel(PlanetUpgradeId.SignalRelay);

        if (relayBonus > 0f)
            multiplier *= 1f + relayBonus;

        return multiplier;
    }






    // -----------------------
    // PROBE MANAGEMENT
    // -----------------------
    // Applies mined Glass and depletes reserves.
    // Dense Strata reduces the depletion rate but does not reduce income.
    private void ApplyGlass(float amount)
    {
        if (amount <= 0f)
            return;

        float reduction = PlanetUpgradeTuning.GetDepletionReduction(GetUpgradeLevel(PlanetUpgradeId.DenseStrata));
        float effectiveDepletion = amount * (1f - reduction);

        currentReserves = Mathf.Max(0f, currentReserves - effectiveDepletion);

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


    // Returns true if there is at least one free probe slot (including upgrades).
    public bool CanAddProbe()
    {
        return probes.Count < GetEffectiveMaxProbeSlots();
    }

    // Adds a probe and registers its deployment timer.
    public void AddProbe(Probe probe)
    {
        if (!CanAddProbe())
            return;

        probes.Add(probe);
        RegisterProbeDeployment(probe);
    }

    // Removes a probe and clears its deployment timer.
    public void RemoveProbe(Probe probe)
    {
        probes.Remove(probe);
        UnregisterProbeDeployment(probe);
    }



        // Returns the upgrade instance if it exists on this planet (null if missing).
    public PlanetUpgradeInstance GetUpgradeInstance(PlanetUpgradeId id)
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i].id == id)
                return upgrades[i];
        }
        return null;
    }

    // Returns the Glass cost to upgrade this upgrade to the next level.
    public double GetUpgradeCost(PlanetUpgradeId id)
    {
        PlanetUpgradeInstance instance = GetUpgradeInstance(id);
        if (instance == null)
            return 0;

        PlanetUpgradeDefinition def = PlanetUpgradeCatalog.Get(id);
        if (def == null || instance.level >= def.maxLevel)
            return 0;

        return PlanetUpgradeCatalog.GetUpgradeCost(id, instance.level);
    }

    // Returns true if the player has enough Glass to upgrade this upgrade.
    public bool CanAffordUpgrade(PlanetUpgradeId id)
    {
        if (GlassManager.Instance == null)
            return false;

        double cost = GetUpgradeCost(id);
        if (cost <= 0)
            return false;

        return GlassManager.Instance.CanAffordGlass(cost);
    }

    // Attempts to upgrade the specified upgrade (spends Glass, increments level).
    public bool TryUpgrade(PlanetUpgradeId id)
    {
        PlanetUpgradeInstance instance = GetUpgradeInstance(id);
        if (instance == null)
            return false;

        PlanetUpgradeDefinition def = PlanetUpgradeCatalog.Get(id);
        if (def == null || instance.level >= def.maxLevel)
            return false;

        double cost = GetUpgradeCost(id);
        if (cost <= 0)
            return false;

        if (GlassManager.Instance == null || !GlassManager.Instance.SpendGlass(cost))
            return false;

        instance.level++;
        glassSpentOnUpgrades += cost;

        if (!upgradeSpendById.ContainsKey(id))
            upgradeSpendById[id] = 0;

        upgradeSpendById[id] += cost;


        return true;
    }



    // Frees a stationary slot and disables its particle effect.
    public void ClearStationarySlot(int index)
    {
        if (index < 0 || index >= stationaryOccupied.Length)
            return;

        stationaryOccupied[index] = false;

        if (index < stationarySlots.Count && stationarySlots[index] != null)
        {
            ProbeStatic staticSlot = stationarySlots[index].GetComponent<ProbeStatic>();
            if (staticSlot != null)
                staticSlot.DeactivateParticleEffect();
        }
    }

    // Frees an orbit slot.
    public void ClearOrbitSlot(int index)
    {
        if (index < 0 || index >= orbitOccupied.Length)
            return;

        orbitOccupied[index] = false;
    }


}
