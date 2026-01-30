using System.Collections.Generic;
using UnityEngine;


public class ProbeManager : MonoBehaviour
{
    public static ProbeManager Instance;

    private Dictionary<ProbeType, ProbeCostData> costs = new Dictionary<ProbeType, ProbeCostData>
    {
        [ProbeType.Extractor] = new ProbeCostData { buyCost = 50, upgradeBaseCost = 25, upgradeCostMultiplier = 1.35f, requiredPlanetTier = 1 },
        [ProbeType.Refinery]  = new ProbeCostData { buyCost = 80, upgradeBaseCost = 40, upgradeCostMultiplier = 1.4f, requiredPlanetTier = 1 },
        [ProbeType.DeepCore]  = new ProbeCostData { buyCost = 120, upgradeBaseCost = 60, upgradeCostMultiplier = 1.45f, requiredPlanetTier = 2 },
        [ProbeType.Amplifier] = new ProbeCostData { buyCost = 150, upgradeBaseCost = 75, upgradeCostMultiplier = 1.5f, requiredPlanetTier = 2 },
        [ProbeType.Survey]    = new ProbeCostData { buyCost = 90, upgradeBaseCost = 45, upgradeCostMultiplier = 1.35f, requiredPlanetTier = 2 },
        [ProbeType.Stabilizer]= new ProbeCostData { buyCost = 110, upgradeBaseCost = 55, upgradeCostMultiplier = 1.4f, requiredPlanetTier = 2 },
        [ProbeType.HeavyMining]= new ProbeCostData { buyCost = 500, upgradeBaseCost = 250, upgradeCostMultiplier = 1.6f, requiredPlanetTier = 4, requiredCores = 2000 }
    };


    [SerializeField] private List<ProbeSpawnData> spawnConfigs = new List<ProbeSpawnData>();
    private Dictionary<ProbeType, ProbeSpawnData> spawnTable;

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

        if (!IsProbeUnlocked(type, planet))
            return false;

        double cost = GetBuyCost(type);
        if (!GlassManager.Instance.SpendGlass(cost))
            return false;

        Probe probe = CreateProbe(type);
        if (probe == null) return false;

        SpawnProbeVisual(probe, planet);

        planet.AddProbe(probe);
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
        }
        else
        {
            if (!planet.TryGetOrbitSlot(out slot, out index))
                return;
        }

        GameObject go = Instantiate(data.prefab, slot.position, slot.rotation, slot);
        ProbeVisual visual = go.GetComponent<ProbeVisual>();
        visuals[probe] = visual;

        if (data.spawnType == ProbeSpawnType.Orbiting)
        {
            var orbit = go.GetComponent<ProbeOrbit>();
            if (orbit == null) orbit = go.AddComponent<ProbeOrbit>();
            orbit.Init(planet.transform, data.orbitSpeed, data.clockwise);
        }

        // initial particles
        visual.SetOutput(probe.GetOutput(planet.BuildContext()));
    }


    public void UpdateProbeVisual(Probe probe, ProbeOutput output)
    {
        if (visuals.TryGetValue(probe, out var visual) && visual != null)
            visual.SetOutput(output);
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

public class ProbeSpawnData
{
    public ProbeType type;
    public ProbeSpawnType spawnType;
    public GameObject prefab;
    public float orbitSpeed;
    public bool clockwise;
}
