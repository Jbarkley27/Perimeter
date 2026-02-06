using System.Collections.Generic;
using UnityEngine;

/*
 * SectorModifierDefinition
 * ------------------------
 * ScriptableObject definition for a run modifier.
 * These are chosen after a sector is cleared.
 */
[CreateAssetMenu(fileName = "SectorModifier", menuName = "Game/Sector Modifier")]
public class SectorModifierDefinition : ScriptableObject
{
    [Header("Identity")]
    public string modifierId;
    public string displayName;
    [TextArea] public string description;

    [Header("Availability")]
    public SectorModifierRarity rarity = SectorModifierRarity.Common;
    public int minSectorReq = 1;
    public bool isHoldCourse = false;

    [Header("Gameplay Effects")]
    public List<SectorModifierEffect> effects = new List<SectorModifierEffect>();

    [Header("Rewards (shown on hover, granted later)")]
    public List<SectorRewardEntry> rewards = new List<SectorRewardEntry>();
    
    [Header("Spawn Bonuses")]
    public List<SectorEnemySpawnBonus> spawnBonuses = new List<SectorEnemySpawnBonus>();

}
