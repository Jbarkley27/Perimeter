using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * PlanetUpgradeCatalog
 * --------------------
 * Central, hard-coded registry for planet upgrades.
 * Keeps tuning values and player-facing descriptions in one place.
 * Designed for easy future conversion to ScriptableObjects.
 */

public enum PlanetUpgradeId
{
    // RESERVES & SUSTAINABILITY
    LargeReserve,
    DeepMantle,
    CrystallineMantle,
    SubsurfaceVents,
    StratifiedCore,
    HardenedCrust,
    DenseStrata,
    ConversionBuffer,

    // PROBE COST, SLOTS & SCALING
    AutomatedCalibration,
    PredictiveLogistics,
    OrbitalExpansion,
    ScalableFramework,
    MassFabrication,

    // PROBE INTERACTION & COMPOSITION
    ResonantLattice,
    SignalRelay,

    // CORE & META GENERATION
    RefinedOutput,
    LongTermYield
}

/*
 * PlanetUpgradeDefinition
 * -----------------------
 * Lightweight definition for one upgrade.
 * Holds tuning, display, and roll metadata.
 */
public class PlanetUpgradeDefinition
{
    public PlanetUpgradeId id;
    public string displayName;
    public int maxLevel;
    public float weight;
    public bool removeFromPoolAfterRoll;

    // Generates readable effect text for the UI at a given level.
    public Func<int, string> effectTextBuilder;

    public string GetEffectText(int level)
    {
        if (effectTextBuilder == null)
            return string.Empty;

        return effectTextBuilder(level);
    }
}

/*
 * PlanetUpgradeTuning
 * -------------------
 * All tuning values in one place for easy balancing.
 */
public static class PlanetUpgradeTuning
{
    // Global upgrade rules (easy to tweak).
    public const int DefaultMaxLevel = 5;
    public const int OrbitalExpansionMaxLevel = 1;

    // Base probe max level before Scalable Framework bonuses.
    public const int BaseProbeMaxLevel = 5;

    public const int DefaultMaxStartingUpgrades = 4;

    public const double UpgradeBaseCost = 20.0;
    public const float UpgradeCostMultiplier = 1.45f;

    public const double RerollCost = 50.0f;

    // Reserve size bonuses (per level).
    public const float LargeReserveBonusPerLevel = 0.15f;
    public const float DeepMantleBonusPerLevel = 0.25f;
    public const float CrystallineMantleBonusPerLevel = 0.35f;

    // Subsurface vents regen rate (fraction of max reserves per second per level).
    public const float ReserveRegenPerSecondPerLevel = 0.003f;

    // Stratified core curve tuning.
    public const float EfficiencyExponentReductionPerLevel = 0.08f;
    public const float MinimumEfficiencyExponent = 0.5f;

    // Hardened crust efficiency floor.
    public const float EfficiencyFloorPerLevel = 0.05f;

    // Dense strata reserve depletion reduction.
    public const float DepletionReductionPerLevel = 0.10f;
    public const float MaxDepletionReduction = 0.60f;

    // Conversion buffer floor (used when reserves are empty).
    public const float ConversionBufferFloorPerLevel = 0.02f;
    public const float ConversionBufferMaxFloor = 0.20f;

    // Probe cost modifiers.
    public const float UpgradeCostReductionPerLevel = 0.10f;
    public const float MaxUpgradeCostReduction = 0.50f;

    public const float MassFabricationDiscountPerProbePerLevel = 0.03f;
    public const float MassFabricationMaxDiscount = 0.40f;

    // Amplifier modifiers.
    public const float ResonantLatticeBonusPerLevel = 0.20f;
    public const float SignalRelayBonusPerLevel = 0.10f;

    // Refinery core output bonus.
    public const float RefinedOutputBonusPerLevel = 0.15f;

    // Long-term yield bonus.
    public const float LongTermYieldBonusPerMinutePerLevel = 0.01f;
    public const float LongTermYieldMaxBonus = 0.30f;

    public static double GetUpgradeCost(int currentLevel)
    {
        // Cost to go from currentLevel -> currentLevel + 1.
        return UpgradeBaseCost * Mathf.Pow(UpgradeCostMultiplier, Mathf.Max(0, currentLevel - 1));
    }

    public static float GetEfficiencyExponent(int level)
    {
        // Lower exponent makes efficiency drop more slowly.
        float exponent = 1f - (EfficiencyExponentReductionPerLevel * level);
        return Mathf.Max(MinimumEfficiencyExponent, exponent);
    }

    public static float GetDepletionReduction(int level)
    {
        return Mathf.Min(DepletionReductionPerLevel * level, MaxDepletionReduction);
    }

    public static float GetConversionBufferFloor(int level)
    {
        return Mathf.Min(ConversionBufferFloorPerLevel * level, ConversionBufferMaxFloor);
    }

    public static float GetUpgradeCostReduction(int level)
    {
        return Mathf.Min(UpgradeCostReductionPerLevel * level, MaxUpgradeCostReduction);
    }

    public static float GetMassFabricationDiscountPerProbe(int level)
    {
        return Mathf.Min(MassFabricationDiscountPerProbePerLevel * level, MassFabricationMaxDiscount);
    }

    public static float GetLongTermYieldBonus(float minutesDeployed, int level)
    {
        float bonus = minutesDeployed * LongTermYieldBonusPerMinutePerLevel * level;
        return Mathf.Min(bonus, LongTermYieldMaxBonus);
    }
}

/*
 * PlanetUpgradeCatalog
 * --------------------
 * Static lookup for all definitions and their weights.
 */
public static class PlanetUpgradeCatalog
{
    private static readonly List<PlanetUpgradeDefinition> definitions = new List<PlanetUpgradeDefinition>
    {
        // RESERVES & SUSTAINABILITY
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.LargeReserve,
            displayName = "Large Reserve",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 60f,
            effectTextBuilder = level => $"Max reserves +{Percent(PlanetUpgradeTuning.LargeReserveBonusPerLevel * level)}."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.DeepMantle,
            displayName = "Deep Mantle",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 40f,
            effectTextBuilder = level => $"Max reserves +{Percent(PlanetUpgradeTuning.DeepMantleBonusPerLevel * level)}."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.CrystallineMantle,
            displayName = "Crystalline Mantle",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 20f,
            effectTextBuilder = level => $"Max reserves +{Percent(PlanetUpgradeTuning.CrystallineMantleBonusPerLevel * level)}."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.SubsurfaceVents,
            displayName = "Subsurface Vents",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 35f,
            effectTextBuilder = level => $"Regenerates {Percent(PlanetUpgradeTuning.ReserveRegenPerSecondPerLevel * level)} max reserves per second while probes are active."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.StratifiedCore,
            displayName = "Stratified Core",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 35f,
            effectTextBuilder = level => "Efficiency declines more slowly as reserves deplete."
        },

        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.HardenedCrust,
            displayName = "Hardened Crust",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 45f,
            effectTextBuilder = level => $"Minimum efficiency +{Percent(PlanetUpgradeTuning.EfficiencyFloorPerLevel * level)}."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.DenseStrata,
            displayName = "Dense Strata",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 40f,
            effectTextBuilder = level => $"Reserves deplete {Percent(PlanetUpgradeTuning.GetDepletionReduction(level))} more slowly."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.ConversionBuffer,
            displayName = "Conversion Buffer",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 25f,
            effectTextBuilder = level => $"When reserves are empty, mining continues at {Percent(PlanetUpgradeTuning.GetConversionBufferFloor(level))} efficiency."
        },

        // PROBE COST, SLOTS & SCALING
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.AutomatedCalibration,
            displayName = "Automated Calibration",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 45f,
            effectTextBuilder = level => $"Probe upgrade costs -{Percent(PlanetUpgradeTuning.GetUpgradeCostReduction(level))}."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.PredictiveLogistics,
            displayName = "Predictive Logistics",
            maxLevel = 1,
            weight = 25f,
            removeFromPoolAfterRoll = true,
            effectTextBuilder = level => "First probe deployed on this planet is free."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.OrbitalExpansion,
            displayName = "Orbital Expansion",
            maxLevel = PlanetUpgradeTuning.OrbitalExpansionMaxLevel,
            weight = 30f,
            effectTextBuilder = level => "+1 probe slot on this planet."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.ScalableFramework,
            displayName = "Scalable Framework",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 30f,
            effectTextBuilder = level => $"Max probe level +{level}."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.MassFabrication,
            displayName = "Mass Fabrication",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 35f,
            effectTextBuilder = level => $"Each existing probe reduces new probe cost by {Percent(PlanetUpgradeTuning.GetMassFabricationDiscountPerProbe(level))} (max {Percent(PlanetUpgradeTuning.MassFabricationMaxDiscount)})."
        },

        // PROBE INTERACTION & COMPOSITION
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.ResonantLattice,
            displayName = "Resonant Lattice",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 30f,
            effectTextBuilder = level => $"Amplifier bonuses are {Percent(PlanetUpgradeTuning.ResonantLatticeBonusPerLevel * level)} stronger."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.SignalRelay,
            displayName = "Signal Relay",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 25f,
            effectTextBuilder = level => $"Adds a bonus amplifier stack worth {Percent(PlanetUpgradeTuning.SignalRelayBonusPerLevel * level)} output."
        },

        // CORE & META GENERATION
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.RefinedOutput,
            displayName = "Refined Output",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 30f,
            effectTextBuilder = level => $"Refinery probes generate {Percent(PlanetUpgradeTuning.RefinedOutputBonusPerLevel * level)} more Cores."
        },
        new PlanetUpgradeDefinition
        {
            id = PlanetUpgradeId.LongTermYield,
            displayName = "Long-Term Yield",
            maxLevel = PlanetUpgradeTuning.DefaultMaxLevel,
            weight = 25f,
            effectTextBuilder = level => $"Probe output increases over time: +{Percent(PlanetUpgradeTuning.LongTermYieldBonusPerMinutePerLevel * level)} per minute (max {Percent(PlanetUpgradeTuning.LongTermYieldMaxBonus)})."
        }
    };

    // Public access to all definitions (read-only).
    public static IReadOnlyList<PlanetUpgradeDefinition> AllDefinitions => definitions;

    public static PlanetUpgradeDefinition Get(PlanetUpgradeId id)
    {
        for (int i = 0; i < definitions.Count; i++)
        {
            if (definitions[i].id == id)
                return definitions[i];
        }
        return null;
    }



    // Cost to upgrade from currentLevel -> currentLevel + 1.
    public static double GetUpgradeCost(PlanetUpgradeId id, int currentLevel)
    {
        PlanetUpgradeDefinition def = Get(id);
        if (def == null || currentLevel >= def.maxLevel)
            return 0;

        return PlanetUpgradeTuning.GetUpgradeCost(currentLevel);
    }

    private static string Percent(float value)
    {
        return $"{value * 100f:0.#}%";
    }
}
