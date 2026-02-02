using System.Collections.Generic;
using UnityEngine;


public class ProbeManager : MonoBehaviour
{
    public static ProbeManager Instance;

    private Dictionary<ProbeType, ProbeCostData> costs = new Dictionary<ProbeType, ProbeCostData>
    {
        [ProbeType.Extractor] = new ProbeCostData { buyCost = 10, upgradeBaseCost = 25, upgradeCostMultiplier = 1.35f, requiredPlanetTier = 0 },
        [ProbeType.Refinery]  = new ProbeCostData { buyCost = 20, upgradeBaseCost = 20, upgradeCostMultiplier = 1.4f, requiredPlanetTier = 0 },
        [ProbeType.DeepCore]  = new ProbeCostData { buyCost = 10, upgradeBaseCost = 60, upgradeCostMultiplier = 1.45f, requiredPlanetTier = 0 },
        [ProbeType.Amplifier] = new ProbeCostData { buyCost = 10, upgradeBaseCost = 75, upgradeCostMultiplier = 1.5f, requiredPlanetTier = 0 },
        [ProbeType.Survey]    = new ProbeCostData { buyCost = 10, upgradeBaseCost = 45, upgradeCostMultiplier = 1.35f, requiredPlanetTier = 0 },
        [ProbeType.Stabilizer]= new ProbeCostData { buyCost = 10, upgradeBaseCost = 55, upgradeCostMultiplier = 1.4f, requiredPlanetTier = 0 },
        [ProbeType.HeavyMining]= new ProbeCostData { buyCost = 20, upgradeBaseCost = 250, upgradeCostMultiplier = 1.6f, requiredPlanetTier = 0, requiredCores = 2000 }
    };


    [SerializeField] private List<ProbeSpawnData> spawnConfigs = new List<ProbeSpawnData>();
    private Dictionary<ProbeType, ProbeSpawnData> spawnTable;

    [Header("Probe Unlocks (Global)")]
    public List<ProbeUnlockEntry> unlockOrder = new List<ProbeUnlockEntry>();

    [Header("Debug")]
    public bool unlockNextProbeDebug;

    private Dictionary<ProbeType, bool> unlockedLookup = new Dictionary<ProbeType, bool>();

    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else { 
            Destroy(gameObject); 
            return; 
        }
        DontDestroyOnLoad(gameObject);


        spawnTable = new Dictionary<ProbeType, ProbeSpawnData>();
        foreach (var cfg in spawnConfigs)
        {
            if (cfg != null)
                spawnTable[cfg.type] = cfg;
        }

        RefreshUnlocks();
    }


    void Update()
    {
        if (unlockNextProbeDebug)
        {
            unlockNextProbeDebug = false;
            UnlockNextProbe();
        }
    }


    // Builds a quick lookup from the inspector list.
    public void RefreshUnlocks()
    {
        unlockedLookup.Clear();
        foreach (var entry in unlockOrder)
        {
            unlockedLookup[entry.type] = entry.unlocked;
        }
    }

    // Returns true if the probe type is unlocked globally.
    public bool IsProbeUnlocked(ProbeType type)
    {
        return unlockedLookup.ContainsKey(type) && unlockedLookup[type];
    }


    // Returns true if the player can afford this probe on a specific planet.
    public bool CanAffordProbe(ProbeType type, Planet planet)
    {
        double cost = GetBuyCost(type, planet);
        return GlassManager.Instance.CanAffordGlass(cost);
    }

    // Backward-compatible helper: uses current planet.
    public bool CanAffordProbe(ProbeType type)
    {
        Planet planet = MiningManager.Instance != null ? MiningManager.Instance.CurrentPlanet : null;
        return CanAffordProbe(type, planet);
    }


    // Unlocks the next locked probe in the list.
    public void UnlockNextProbe()
    {
        foreach (var entry in unlockOrder)
        {
            if (!entry.unlocked)
            {
                entry.unlocked = true;
                break;
            }
        }
        RefreshUnlocks();
    }



    public Probe CreateProbe(ProbeType type)
    {
        switch (type)
        {
            case ProbeType.Extractor: return new ExtractorProbe();
            case ProbeType.Refinery: return new RefineryProbe();
            case ProbeType.DeepCore: return new DeepCoreProbe();
            case ProbeType.Amplifier: return new AmplifierProbe();
            case ProbeType.Survey: return new SurveyProbe();
            case ProbeType.Stabilizer: return new StabilizerProbe();
            case ProbeType.HeavyMining: return new HeavyMiningProbe();
            default: return null;
        }
    }


    // Base buy cost without planet modifiers.
    public double GetBuyCost(ProbeType type)
    {
        return costs[type].buyCost;
    }

    // Buy cost with planet modifiers (Mass Fabrication).
    public double GetBuyCost(ProbeType type, Planet planet)
    {
        double baseCost = GetBuyCost(type);
        if (planet == null)
            return baseCost;

        float discountPerProbe = planet.GetProbeBuyCostDiscountPerProbe();
        int existingProbes = planet.Probes.Count;

        float discount = Mathf.Min(
            discountPerProbe * existingProbes,
            PlanetUpgradeTuning.MassFabricationMaxDiscount
        );

        return baseCost * (1f - discount);
    }


     // Base upgrade cost without planet modifiers.
    public double GetUpgradeCost(Probe probe)
    {
        ProbeCostData data = costs[probe.Type];

        // Level 1 should cost base (multiplier^0).
        int exponent = Mathf.Max(0, probe.Level - 1);
        return data.upgradeBaseCost * Mathf.Pow(data.upgradeCostMultiplier, exponent);
    }


    // Upgrade cost with planet modifiers (Automated Calibration).
    public double GetUpgradeCost(Probe probe, Planet planet)
    {
        double baseCost = GetUpgradeCost(probe);
        if (planet == null)
            return baseCost;

        float reduction = planet.GetProbeUpgradeCostReduction();
        return baseCost * (1f - reduction);
    }



    public bool IsProbeUnlocked(ProbeType type, Planet planet)
    {
        ProbeCostData data = costs[type];
        // if (planet.planetTier < data.requiredPlanetTier)
        //     return false;

        // TODO: add this back once economy or probe gating is finalized
        // This currently allows testing of Heavy Mining probe without needing cores.
        // if (data.requiredCores > 0 && (CoreManager.Instance == null || CoreManager.Instance.totalCores < data.requiredCores))
        //     return false;

        // prestige gating placeholder if you add it later
        return true;
    }


    public bool PurchaseProbe(ProbeType type, Planet planet)
    {
        if (planet == null || !planet.CanAddProbe())
            return false;

        Debug.Log("Planet not null and can add probe");
        if (!IsProbeUnlocked(type, planet))
        {
            Debug.Log("Probe type not unlocked for this planet");
            return false;
        }

        double cost = GetBuyCost(type, planet);

        // Predictive Logistics: first probe is free on this planet.
        bool useFreeProbe = planet != null && planet.CanUsePredictiveLogisticsFreeProbe();
        if (useFreeProbe)
            cost = 0;

        if (!GlassManager.Instance.SpendGlass(cost))
        {
            Debug.Log("Not enough glass to buy probe");
            return false;
        }
        

        if (useFreeProbe)
            planet.UsePredictiveLogisticsFreeProbe();



        Probe probe = CreateProbe(type);


        RecordPurchaseCost(probe, cost);

        if (probe == null) return false;

        SpawnProbeVisual(probe, planet);

        planet.AddProbe(probe);
        MiningManager.Instance.miningUI.RefreshPlanetUI(MiningManager.Instance.CurrentPlanet);
        return true;
    }

    // Upgrades a probe using planet-specific rules (cost reduction + max level).
    public bool UpgradeProbe(Probe probe, Planet planet)
    {
        if (probe == null)
            return false;

        if (planet != null)
        {
            int maxLevel = planet.GetMaxProbeLevel();
            if (probe.Level >= maxLevel)
                return false;
        }

        double cost = GetUpgradeCost(probe, planet);
        if (!GlassManager.Instance.SpendGlass(cost))
            return false;


        RecordUpgradeCost(probe, cost);
        probe.Upgrade();
        return true;
    }

    // Backward-compatible helper: uses current planet.
    public bool UpgradeProbe(Probe probe)
    {
        Planet planet = MiningManager.Instance != null ? MiningManager.Instance.CurrentPlanet : null;
        return UpgradeProbe(probe, planet);
    }




    private Dictionary<Probe, ProbeVisual> visuals = new();

    public void SpawnProbeVisual(Probe probe, Planet planet)
    {
        if (!spawnTable.TryGetValue(probe.Type, out var data))
            return;

        Transform slot;
        int index;

        if (data.spawnType == ProbeSpawnType.Stationary)
        {
            if (!planet.TryGetStationarySlot(out slot, out index))
                return;

            SpawnStationary(slot, data, probe, index);

            slotBindings[probe] = new ProbeSlotBinding
            {
                planet = planet,
                spawnType = ProbeSpawnType.Stationary,
                slotIndex = index,
                slotTransform = slot
            };

            return;

        }
        else
        {
            if (!planet.TryGetOrbitSlot(out slot, out index))
                return;
        }

        GameObject go = Instantiate(data.prefab, slot.position, slot.rotation, slot);
        ProbeVisual visual = go.GetComponent<ProbeVisual>();
        visuals[probe] = visual;

        slotBindings[probe] = new ProbeSlotBinding
        {
            planet = planet,
            spawnType = ProbeSpawnType.Orbiting,
            slotIndex = index,
            slotTransform = slot
        };


        Debug.Log("Got probe visual");
        if (data.spawnType == ProbeSpawnType.Orbiting)
        {
            var orbit = slot.GetComponent<ProbeOrbit>();
            if (orbit == null) orbit = slot.gameObject.AddComponent<ProbeOrbit>();
            orbit.Init(planet.transform, data.orbitSpeed, data.clockwise, visual.gameObject);
        }

        // initial particles
        // TODO: UPDATE SO IT USES PROBE OUTPUT
        // visual.SetOutput(probe.GetOutput(planet.BuildContext()));
    }


    public void UpdateProbeVisual(Probe probe, ProbeOutput output)
    {
        if (visuals.TryGetValue(probe, out var visual) && visual != null)
            visual.SetOutput(output);
    }


        // Spawns a stationary probe at the slot’s configured visual anchor.
    private void SpawnStationary(Transform slot, ProbeSpawnData data, Probe probe, int index)
    {
        ProbeStatic staticSlot = slot.GetComponent<ProbeStatic>();
        if (staticSlot == null)
            return;

        GameObject go = Instantiate(data.prefab, staticSlot.transform.position, Quaternion.identity, staticSlot.transform);
        ProbeVisual visual = go.GetComponent<ProbeVisual>();
        visuals[probe] = visual;

        staticSlot.ActiveParticleEffect();
    }



    // Returns the configured icon for a probe type (or null if missing).
    public Sprite GetProbeIcon(ProbeType type)
    {
        if (spawnTable != null && spawnTable.TryGetValue(type, out var data))
            return data.icon;

        return null;
    }



    // Tracks actual Glass spent on a probe so refunds are accurate.
    private class ProbeSpendData
    {
        public double purchaseCost;
        public List<double> upgradeCosts = new List<double>();

        public double TotalSpent
        {
            get
            {
                double total = purchaseCost;
                for (int i = 0; i < upgradeCosts.Count; i++)
                    total += upgradeCosts[i];
                return total;
            }
        }
    }

    // Tracks which slot a probe occupies so we can free it on refund.
    private struct ProbeSlotBinding
    {
        public Planet planet;
        public ProbeSpawnType spawnType;
        public int slotIndex;
        public Transform slotTransform;
    }

    private readonly Dictionary<Probe, ProbeSpendData> spendData = new Dictionary<Probe, ProbeSpendData>();
    private readonly Dictionary<Probe, ProbeSlotBinding> slotBindings = new Dictionary<Probe, ProbeSlotBinding>();


        // Gets existing spend data or rebuilds a best-effort estimate if missing.
    private ProbeSpendData GetOrCreateSpendData(Probe probe, Planet planet)
    {
        if (probe == null)
            return null;

        if (spendData.TryGetValue(probe, out var data))
            return data;

        // Best-effort reconstruction (if probe existed before tracking).
        data = new ProbeSpendData();
        data.purchaseCost = GetBuyCost(probe.Type, planet);

        // Recreate upgrade costs by simulating each past level-up.
        ProbeCostData costData = costs[probe.Type];
        float reduction = planet != null ? planet.GetProbeUpgradeCostReduction() : 0f;

        // Rebuild only the upgrades above level 1.
        int upgradesPurchased = Mathf.Max(0, probe.Level - 1);
        for (int level = 0; level < upgradesPurchased; level++)
        {
            double cost = costData.upgradeBaseCost * Mathf.Pow(costData.upgradeCostMultiplier, level);
            cost *= (1f - reduction);
            data.upgradeCosts.Add(cost);
        }


        spendData[probe] = data;
        return data;
    }


    // Returns true if the player can afford the next upgrade for this probe.
    public bool CanAffordUpgrade(Probe probe, Planet planet)
    {
        if (probe == null)
            return false;

        if (planet != null && probe.Level >= planet.GetMaxProbeLevel())
            return false;

        double cost = GetUpgradeCost(probe, planet);
        return GlassManager.Instance != null && GlassManager.Instance.CanAffordGlass(cost);
    }


    // Records the purchase cost for a new probe.
    private void RecordPurchaseCost(Probe probe, double cost)
    {
        if (probe == null)
            return;

        ProbeSpendData data = GetOrCreateSpendData(probe, null);
        if (data == null)
            return;

        data.purchaseCost = cost;
    }

    // Records the upgrade cost for a probe.
    private void RecordUpgradeCost(Probe probe, double cost)
    {
        if (probe == null)
            return;

        ProbeSpendData data = GetOrCreateSpendData(probe, null);
        if (data == null)
            return;

        data.upgradeCosts.Add(cost);
    }


        // Returns the full spend on this probe (purchase + upgrades).
    public double GetProbeTotalSpent(Probe probe, Planet planet)
    {
        ProbeSpendData data = GetOrCreateSpendData(probe, planet);
        return data != null ? data.TotalSpent : 0;
    }

    // Returns the refund amount based on the configured refund percentage.
    public double GetProbeRefundAmount(Probe probe, Planet planet)
    {
        double total = GetProbeTotalSpent(probe, planet);
        float refundPercent = MiningManager.Instance != null
            ? MiningManager.Instance.probeRefundPercent
            : 1f;

        return total * refundPercent;
    }

    // Refunds a probe and removes it from the planet.
    public bool RefundProbe(Probe probe, Planet planet)
    {
        if (probe == null || planet == null)
            return false;

        double refundAmount = GetProbeRefundAmount(probe, planet);

        if (GlassManager.Instance != null && refundAmount > 0)
            GlassManager.Instance.AddGlass(refundAmount);

        // Despawn visual + free slot.
        DespawnProbeVisual(probe);

        // Remove from planet data.
        planet.RemoveProbe(probe);

        // Clean up spend tracking.
        spendData.Remove(probe);

        // Refresh UI.
        if (MiningManager.Instance != null && MiningManager.Instance.miningUI != null)
            MiningManager.Instance.miningUI.RefreshPlanetUI(planet);

        return true;
    }

    // Destroys the probe visual and frees its slot.
    private void DespawnProbeVisual(Probe probe)
    {
        if (probe == null)
            return;

        if (visuals.TryGetValue(probe, out var visual) && visual != null)
        {
            Destroy(visual.gameObject);
            visuals.Remove(probe);
        }

        if (slotBindings.TryGetValue(probe, out var binding))
        {
            if (binding.planet != null)
            {
                if (binding.spawnType == ProbeSpawnType.Stationary)
                    binding.planet.ClearStationarySlot(binding.slotIndex);
                else
                    binding.planet.ClearOrbitSlot(binding.slotIndex);
            }

            slotBindings.Remove(probe);
        }
    }


}




public class ProbeCostData
{
    public double buyCost;
    public double upgradeBaseCost;
    public float upgradeCostMultiplier;
    public int requiredPlanetTier;
    public double requiredCores;
    public int requiredPrestige; // placeholder
}


public enum ProbeSpawnType { Stationary, Orbiting }

[System.Serializable]
public class ProbeSpawnData
{
    public ProbeType type;
    public ProbeSpawnType spawnType;
    public GameObject prefab;
    public float orbitSpeed;
    public bool clockwise;

    // Icon used by UI buttons / slots.
    public Sprite icon;
}
