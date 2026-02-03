using System;
using UnityEngine;

/*
 * SectorTypes
 * -----------
 * Shared enums and data structs for the sector/compass system.
 * Keep these lightweight and UI-friendly.
 */
public enum SectorDirection
{
    North,
    East,
    South,
    West
}

public enum SectorModifierRarity
{
    Common,
    Uncommon,
    Rare
}

/*
 * SectorRewardEntry
 * -----------------
 * Descriptive reward entries shown in UI.
 * Actual reward payout is handled by the run/sector system later.
 */
[Serializable]
public struct SectorRewardEntry
{
    public string rewardName;
    [TextArea] public string rewardDescription;
}

/*
 * SectorModifierEffectType
 * ------------------------
 * Generic effect types that sector modifiers can apply.
 * Not all are wired yet; these are the hooks for future systems.
 */
public enum SectorModifierEffectType
{
    None = 0,
    AllDamageMultiplier,
    EnemyDamageMultiplier,
    EnemyHealthMultiplier,
    PlayerDamageMultiplier,
    PlayerHealthMultiplier,
    PlayerBarrierMultiplier,
    PlayerBarrierRegenMultiplier,
    PlayerCooldownMultiplier,
    ProbeEffectivenessMultiplier,
    GlassEarnedMultiplier,
    GlassOnClearPercentOfCurrent,
    GlassOnClearPercentOfTotal,
    GlassOnWaveClearFlat,
    GlassOnStatusAppliedFlat,
    StatusApplyChanceMultiplier,
    StatusEffectStrengthMultiplier,
    ElementDamageMultiplier,
    ElementGlassMultiplier,
    EnemyAttackSpeedMultiplier,
    PlayerCritChanceMultiplier,
    EnemyCritImmune,
    AugmentChanceBonus
}

/*
 * SectorModifierEffect
 * --------------------
 * A single effect entry; value interpretation depends on effectType.
 * For multipliers: 1.0 = no change, 1.2 = +20%, 0.8 = -20%.
 */
[Serializable]
public struct SectorModifierEffect
{
    public SectorModifierEffectType effectType;
    public float value;
    public Element element; // used for element-specific effects

    // Debug helper for logs / inspector sanity.
    public string GetDebugLabel()
    {
        return effectType.ToString();
    }
}
