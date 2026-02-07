using System.Collections.Generic;
using UnityEngine;

/*
 * SectorManager
 * -------------
 * Owns sector progression, modifier selection state, and exposes
 * runtime queries for sector difficulty + active modifiers.
 */

public class SectorManager : MonoBehaviour
{
    public static SectorManager Instance { get; private set; }

    [Header("Sector Data")]
    public List<Sector> Sectors = new List<Sector>();
    public int currentSectorIndex = 0;

    // Modifier definitions live here (used in later steps).
    [Header("Modifier Definitions")]
    public List<SectorModifierDefinition> modifierDefinitions = new List<SectorModifierDefinition>();


    [Header("Compass Settings")]
    [Range(1, 4)] public int compassChoiceCount = 3;
    public bool forceHoldCourseChoice = true;

    [Header("Runtime State")]
    [SerializeField] private List<SectorCompassChoice> pendingCompassChoices = new List<SectorCompassChoice>();
    [SerializeField] private SectorModifierDefinition activeModifier;
    [SerializeField] private List<SectorModifierEffect> activeEffects = new List<SectorModifierEffect>();
    [SerializeField] private List<SectorDecisionRecord> decisionHistory = new List<SectorDecisionRecord>();


    public IReadOnlyList<SectorCompassChoice> PendingCompassChoices => pendingCompassChoices;
    public IReadOnlyList<SectorDecisionRecord> DecisionHistory => decisionHistory;
    public SectorModifierDefinition ActiveModifier => activeModifier;
    public IReadOnlyList<SectorModifierEffect> ActiveEffects => activeEffects;

    [Header("Sector Difficulty Scaling")]
    public float baseSectorDifficulty = 1f;
    public float difficultyPerSector = 0.1f;
    public float maxSectorDifficulty = 5f;




    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Total sectors are driven by the data list.
    public int GetMaxSectorCount()
    {
        return Mathf.Max(1, Sectors != null ? Sectors.Count : 0);
    }

    public Sector GetCurrentSector()
    {
        if (Sectors == null || Sectors.Count == 0)
            return null;

        int clamped = Mathf.Clamp(currentSectorIndex, 0, Sectors.Count - 1);
        return Sectors[clamped];
    }

    public void AdvanceToNextSector()
    {
        int maxCount = GetMaxSectorCount();
        if (currentSectorIndex < maxCount - 1)
        {
            currentSectorIndex++;
            Debug.Log("Advanced to Sector: " + currentSectorIndex);
        }
        else
        {
            Debug.Log("Already at maximum sector.");
        }
    }

    public void ResetSectors()
    {
        currentSectorIndex = 0;

        ClearPendingCompassChoices();
        ClearRunModifierState();
        decisionHistory.Clear();

        Debug.Log("Sectors reset to Sector: " + currentSectorIndex);
    }


    public bool IsAtMaxSector()
    {
        return currentSectorIndex >= GetMaxSectorCount() - 1;
    }

    // 1-based for UI.
    public int GetCurrentSectorIndex()
    {
        return currentSectorIndex + 1;
    }

    // 1-based for UI.
    public int GetNextSectorIndex()
    {
        int nextIndex = Mathf.Min(currentSectorIndex + 1, GetMaxSectorCount() - 1);
        return nextIndex + 1;
    }

    /*
     * HasPendingCompassChoices
     * ------------------------
     * Used by UI to decide whether to show the existing choice set or roll a new one.
     */
    public bool HasPendingCompassChoices()
    {
        return pendingCompassChoices != null && pendingCompassChoices.Count > 0;
    }

    /*
     * EnsurePendingCompassChoices
     * ---------------------------
     * Only generates choices if none exist (so the choice persists across console visits).
     */
    public void EnsurePendingCompassChoices(int nextSectorNumber)
    {
        if (HasPendingCompassChoices())
            return;

        GeneratePendingCompassChoices(nextSectorNumber);
    }

    /*
     * ClearPendingCompassChoices
     * --------------------------
     * Clears the stored choices once a direction is selected.
     */
    public void ClearPendingCompassChoices()
    {
        pendingCompassChoices.Clear();
    }

    /*
     * ClearRunModifierState
     * ---------------------
     * Clears active modifier state (call when resetting the run).
     */
    public void ClearRunModifierState()
    {
        activeModifier = null;
        activeEffects.Clear();

        if (StatsManager.Instance != null)
            StatsManager.Instance.ResetSectorModifiers();

        if (SkillTreeData.Instance != null)
            SkillTreeData.Instance.RebuildAll();

    }

    /*
     * SelectCompassChoice
     * -------------------
     * Applies the modifier, stores history, and clears pending choices.
     */
    public void SelectCompassChoice(SectorCompassChoice choice)
    {
        // Hold Course should behave like "no active modifier".
        if (choice.modifier != null && choice.modifier.isHoldCourse)
        {
            activeModifier = null;
            activeEffects.Clear();

            ApplyActiveModifiersToPlayer(); // resets sector modifiers to base

            decisionHistory.Add(new SectorDecisionRecord
            {
                sectorIndex = currentSectorIndex,
                direction = choice.direction,
                modifierId = "HOLD_COURSE"
            });

            ClearPendingCompassChoices();
            return;
        }

        activeModifier = choice.modifier;
        activeEffects.Clear();

        if (choice.modifier != null)
            activeEffects.AddRange(choice.modifier.effects);


        ApplyActiveModifiersToPlayer();


        decisionHistory.Add(new SectorDecisionRecord
        {
            sectorIndex = currentSectorIndex,
            direction = choice.direction,
            modifierId = choice.modifier != null ? choice.modifier.modifierId : "HOLD_COURSE"
        });

        ClearPendingCompassChoices();
    }

    /*
     * TryGetChoiceForDirection
     * ------------------------
     * Finds the pending choice tied to a compass direction.
     */
    public bool TryGetChoiceForDirection(SectorDirection direction, out SectorCompassChoice choice)
    {
        for (int i = 0; i < pendingCompassChoices.Count; i++)
        {
            if (pendingCompassChoices[i].direction == direction)
            {
                choice = pendingCompassChoices[i];
                return true;
            }
        }

        choice = default;
        return false;
    }

    /*
     * GeneratePendingCompassChoices
     * -----------------------------
     * Builds a set of compass choices for the next sector.
     * Includes Hold Course if forceHoldCourseChoice is true.
     */
    public void GeneratePendingCompassChoices(int nextSectorNumber)
    {
        pendingCompassChoices.Clear();

        List<SectorModifierDefinition> pool = GetAvailableModifiers(nextSectorNumber);
        SectorModifierDefinition holdCourse = GetHoldCourseDefinition();

        if (forceHoldCourseChoice && holdCourse != null)
            AddChoice(holdCourse);

        if (holdCourse != null)
            pool.Remove(holdCourse);

        int totalChoices = Mathf.Clamp(compassChoiceCount, 1, 4);
        int remainingSlots = totalChoices - (forceHoldCourseChoice && holdCourse != null ? 1 : 0);

        for (int i = 0; i < remainingSlots; i++)
        {
            if (pool.Count <= 0)
                break;

            SectorModifierDefinition picked = PickWeightedModifier(pool);
            if (picked == null)
                break;

            pool.Remove(picked);
            AddChoice(picked);
        }

        AssignDirectionsToChoices();
    }

    /*
     * GetAvailableModifiers
     * ---------------------
     * Returns all modifiers unlocked at the given sector number.
     */
    private List<SectorModifierDefinition> GetAvailableModifiers(int sectorNumber)
    {
        List<SectorModifierDefinition> pool = new List<SectorModifierDefinition>();

        for (int i = 0; i < modifierDefinitions.Count; i++)
        {
            SectorModifierDefinition def = modifierDefinitions[i];
            if (def == null)
                continue;

            if (sectorNumber >= def.minSectorReq)
                pool.Add(def);
        }

        return pool;
    }

    /*
     * GetHoldCourseDefinition
     * -----------------------
     * Finds the "Hold Course" modifier definition.
     */
    private SectorModifierDefinition GetHoldCourseDefinition()
    {
        for (int i = 0; i < modifierDefinitions.Count; i++)
        {
            SectorModifierDefinition def = modifierDefinitions[i];
            if (def != null && def.isHoldCourse)
                return def;
        }

        return null;
    }

    /*
     * AddChoice
     * ---------
     * Adds a choice with a placeholder direction (assigned later).
     */
    private void AddChoice(SectorModifierDefinition modifier)
    {
        pendingCompassChoices.Add(new SectorCompassChoice
        {
            direction = SectorDirection.North,
            modifier = modifier
        });
    }

    /*
     * AssignDirectionsToChoices
     * -------------------------
     * Randomly assigns N/E/S/W to the current pending choices.
     */
    private void AssignDirectionsToChoices()
    {
        List<SectorDirection> directions = new List<SectorDirection>
        {
            SectorDirection.North,
            SectorDirection.East,
            SectorDirection.South,
            SectorDirection.West
        };

        Shuffle(directions);

        int count = Mathf.Min(pendingCompassChoices.Count, directions.Count);
        for (int i = 0; i < count; i++)
        {
            SectorCompassChoice choice = pendingCompassChoices[i];
            choice.direction = directions[i];
            pendingCompassChoices[i] = choice;
        }
    }

    /*
     * PickWeightedModifier
     * --------------------
     * Chooses a modifier from the pool using rarity weighting.
     */
    private SectorModifierDefinition PickWeightedModifier(List<SectorModifierDefinition> pool)
    {
        float totalWeight = 0f;
        for (int i = 0; i < pool.Count; i++)
            totalWeight += GetRarityWeight(pool[i].rarity);

        if (totalWeight <= 0f)
            return null;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            cumulative += GetRarityWeight(pool[i].rarity);
            if (roll <= cumulative)
                return pool[i];
        }

        return pool[pool.Count - 1];
    }

    /*
     * GetRarityWeight
     * ----------------
     * Weights for Common/Uncommon/Rare.
     * Adjust values as needed for feel.
     */
    private float GetRarityWeight(SectorModifierRarity rarity)
    {
        switch (rarity)
        {
            case SectorModifierRarity.Common:
                return 1f;
            case SectorModifierRarity.Uncommon:
                return 0.6f;
            case SectorModifierRarity.Rare:
                return 0.3f;
            default:
                return 1f;
        }
    }

    /*
     * Shuffle
     * -------
     * Simple Fisher-Yates shuffle.
     */
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }


        // Returns a sector by 0-based index (clamped).
    public Sector GetSectorByIndex(int index)
    {
        if (Sectors == null || Sectors.Count == 0)
            return null;

        int clamped = Mathf.Clamp(index, 0, Sectors.Count - 1);
        return Sectors[clamped];
    }

    // Returns the upcoming sector (based on current index + 1).
    public Sector GetNextSector()
    {
        int nextIndex = Mathf.Min(currentSectorIndex + 1, GetMaxSectorCount() - 1);
        return GetSectorByIndex(nextIndex);
    }




    // Returns the difficulty multiplier for a given sector index (0-based).
    public float GetSectorDifficultyMultiplier(int sectorIndex)
    {
        float value = baseSectorDifficulty + (sectorIndex * difficultyPerSector);
        return Mathf.Clamp(value, baseSectorDifficulty, maxSectorDifficulty);
    }

    // Current sector difficulty.
    public float GetCurrentSectorDifficultyMultiplier()
    {
        return GetSectorDifficultyMultiplier(currentSectorIndex);
    }

    // Next sector difficulty (used when previewing choices).
    public float GetNextSectorDifficultyMultiplier()
    {
        int nextIndex = Mathf.Min(currentSectorIndex + 1, GetMaxSectorCount() - 1);
        return GetSectorDifficultyMultiplier(nextIndex);
    }





    /*
     * Modifier query helpers
     * ----------------------
     * These functions let other systems read the active modifier effects
     * without knowing the raw data structure.
     */

    // True if a modifier effect exists (element checks only apply for element-based effects).
    public bool HasModifierEffect(SectorModifierEffectType type, Element element = Element.Kinetic)
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            SectorModifierEffect effect = activeEffects[i];
            if (effect.effectType != type)
                continue;

            if (IsElementEffect(type) && effect.element != element)
                continue;

            return true;
        }

        return false;
    }

    // Multiplier effects: returns product of all matching effect values.
    public float GetModifierMultiplier(SectorModifierEffectType type, Element element = Element.Kinetic)
    {
        float multiplier = 1f;

        for (int i = 0; i < activeEffects.Count; i++)
        {
            SectorModifierEffect effect = activeEffects[i];
            if (effect.effectType != type)
                continue;

            if (IsElementEffect(type) && effect.element != element)
                continue;

            multiplier *= effect.value;
        }

        return multiplier;
    }

    // Additive effects: returns sum of all matching effect values.
    public float GetModifierAdditiveValue(SectorModifierEffectType type, Element element = Element.Kinetic)
    {
        float value = 0f;

        for (int i = 0; i < activeEffects.Count; i++)
        {
            SectorModifierEffect effect = activeEffects[i];
            if (effect.effectType != type)
                continue;

            if (IsElementEffect(type) && effect.element != element)
                continue;

            value += effect.value;
        }

        return value;
    }

    // Helper: which effect types are element-specific?
    private bool IsElementEffect(SectorModifierEffectType type)
    {
        return type == SectorModifierEffectType.ElementDamageMultiplier
            || type == SectorModifierEffectType.ElementGlassMultiplier;
    }



    /*
     * Runtime multiplier helpers
     * ---------------------------
     * These combine sector difficulty + active modifier effects.
     */

    // Enemy health scaling for the current sector.
    public float GetEnemyHealthMultiplier()
    {
        float multiplier = GetCurrentSectorDifficultyMultiplier();
        multiplier *= GetModifierMultiplier(SectorModifierEffectType.EnemyHealthMultiplier);
        return multiplier;
    }

    // Enemy damage scaling for the current sector (uses element when relevant).
    public float GetEnemyDamageMultiplier(Element element = Element.Kinetic)
    {
        float multiplier = GetCurrentSectorDifficultyMultiplier();
        multiplier *= GetModifierMultiplier(SectorModifierEffectType.EnemyDamageMultiplier);
        multiplier *= GetModifierMultiplier(SectorModifierEffectType.AllDamageMultiplier);
        multiplier *= GetModifierMultiplier(SectorModifierEffectType.ElementDamageMultiplier, element);
        return multiplier;
    }

    // Player damage scaling (element‑aware).
    public float GetPlayerDamageMultiplier(Element element = Element.Kinetic)
    {
        float multiplier = 1f;
        multiplier *= GetModifierMultiplier(SectorModifierEffectType.PlayerDamageMultiplier);
        multiplier *= GetModifierMultiplier(SectorModifierEffectType.AllDamageMultiplier);
        multiplier *= GetModifierMultiplier(SectorModifierEffectType.ElementDamageMultiplier, element);
        return multiplier;
    }

    // Glass reward scaling (general, not element‑aware yet).
    public float GetGlassEarnedMultiplier()
    {
        return GetModifierMultiplier(SectorModifierEffectType.GlassEarnedMultiplier);
    }



    /*
     * Applies active sector modifiers to player stats and skills.
     * Called after a compass choice is selected.
     */
    public void ApplyActiveModifiersToPlayer()
    {
        // Rebuild skills to base + skill tree effects first.
        if (SkillTreeData.Instance != null)
            SkillTreeData.Instance.RebuildAll();

        ApplyActiveModifiersToPlayerStats();
        ApplyActiveModifiersToSkills();
    }

    // Applies stat-layer sector modifiers (health, barrier, crit).
    private void ApplyActiveModifiersToPlayerStats()
    {
        if (StatsManager.Instance == null)
            return;

        StatsManager.Instance.ResetSectorModifiers();

        float healthMult = GetModifierMultiplier(SectorModifierEffectType.PlayerHealthMultiplier);
        if (!Mathf.Approximately(healthMult, 1f))
            StatsManager.Instance.ApplySectorModifier(StatsManager.StatType.HEALTH, 0, healthMult - 1f);

        float barrierMult = GetModifierMultiplier(SectorModifierEffectType.PlayerBarrierMultiplier);
        if (!Mathf.Approximately(barrierMult, 1f))
            StatsManager.Instance.ApplySectorModifier(StatsManager.StatType.BARRIER, 0, barrierMult - 1f);

        float critMult = GetModifierMultiplier(SectorModifierEffectType.PlayerCritChanceMultiplier);
        if (!Mathf.Approximately(critMult, 1f))
            StatsManager.Instance.ApplySectorModifier(StatsManager.StatType.CRIT_CHANCE, 0, critMult - 1f);

        // NOTE: Barrier regen multiplier not wired yet (no regen system).

        if (GlobalDataStore.Instance != null && GlobalDataStore.Instance.BarrierModule != null)
            GlobalDataStore.Instance.BarrierModule.ResetHealthBarrier();
    }

    // Applies sector modifiers directly to skill runtime values (cooldowns).
    private void ApplyActiveModifiersToSkills()
    {
        if (SkillTreeData.Instance == null)
            return;

        float cooldownMult = GetModifierMultiplier(SectorModifierEffectType.PlayerCooldownMultiplier);
        if (Mathf.Approximately(cooldownMult, 1f))
            return;

        foreach (var skill in SkillTreeData.Instance.allSkills)
        {
            if (skill == null)
                continue;

            skill.cooldownRate *= cooldownMult;
            skill.cooldownRestartDelay *= cooldownMult;
        }
    }



    // Returns active spawn bonuses (empty if none).
    public List<SectorEnemySpawnBonus> GetActiveSpawnBonuses()
    {
        if (activeModifier == null || activeModifier.spawnBonuses == null)
            return new List<SectorEnemySpawnBonus>();

        return activeModifier.spawnBonuses;
    }

}



/*
 * SectorCompassChoice
 * -------------------
 * A single compass option for the next sector.
 */
[System.Serializable]
public struct SectorCompassChoice
{
    public SectorDirection direction;
    public SectorModifierDefinition modifier;
}

/*
 * SectorDecisionRecord
 * --------------------
 * Keeps a history of the player's choices for progression tracking.
 */
[System.Serializable]
public struct SectorDecisionRecord
{
    public int sectorIndex;
    public SectorDirection direction;
    public string modifierId;
}
