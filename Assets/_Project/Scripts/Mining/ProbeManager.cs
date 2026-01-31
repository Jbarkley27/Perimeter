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


    public double GetBuyCost(ProbeType type)
    {
        return costs[type].buyCost;
    }

    public double GetUpgradeCost(Probe probe)
    {
        ProbeCostData data = costs[probe.Type];
        return data.upgradeBaseCost * Mathf.Pow(data.upgradeCostMultiplier, probe.Level);
    }


    public bool IsProbeUnlocked(ProbeType type, Planet planet)
    {
        ProbeCostData data = costs[type];
        if (planet.planetTier < data.requiredPlanetTier)
            return false;

        if (data.requiredCores > 0 && (CoreManager.Instance == null || CoreManager.Instance.totalCores < data.requiredCores))
            return false;

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

        double cost = GetBuyCost(type);
        if (!GlassManager.Instance.SpendGlass(cost))
        {
            Debug.Log("Not enough glass to buy probe");
            return false;
        }

        Probe probe = CreateProbe(type);
        if (probe == null) return false;

        SpawnProbeVisual(probe, planet);

        planet.AddProbe(probe);
        MiningManager.Instance.miningUI.RefreshPlanetUI(MiningManager.Instance.CurrentPlanet);
        return true;
    }

    public bool UpgradeProbe(Probe probe)
    {
        if (probe == null)
            return false;

        double cost = GetUpgradeCost(probe);
        if (!GlassManager.Instance.SpendGlass(cost))
            return false;


        probe.Upgrade();
        return true;
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

            SpawnStationary(slot, data, probe);
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
    private void SpawnStationary(Transform slot, ProbeSpawnData data, Probe probe)
    {
        ProbeStatic staticSlot = slot.GetComponent<ProbeStatic>();
        Transform spawn = staticSlot != null ? staticSlot.GetSpawnTransform() : slot;

        GameObject go = Instantiate(data.prefab, spawn.position, spawn.rotation, spawn);
        ProbeVisual visual = go.GetComponent<ProbeVisual>();
        visuals[probe] = visual;
    }



    // Returns the configured icon for a probe type (or null if missing).
    public Sprite GetProbeIcon(ProbeType type)
    {
        if (spawnTable != null && spawnTable.TryGetValue(type, out var data))
            return data.icon;

        return null;
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
