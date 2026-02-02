using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Manages Mining tab UI: planet name, probe slots, and rate display.
public class MiningUIController : MonoBehaviour
{
    public TMP_Text planetNameText;
    public TMP_Text glassPerSecText;
    public TMP_Text coresPerSecText;
    public TMP_Text globalGlassPerSecText;
    public TMP_Text globalCoresPerSecText;
    public List<ProbeSlotUI> slotUIs = new List<ProbeSlotUI>();
    public List<ProbeBuyButton> buyButtons = new List<ProbeBuyButton>();
    public Slider reserveSlider;
    public PlanetUpgradeUIController upgradeUI;
    public ProbeUIController probeUI;
    public ProbePurchaseUIController probePurchaseUI;





    void OnEnable()
    {
        if (MiningManager.Instance == null) return;
        
        RefreshPlanetUI(MiningManager.Instance.CurrentPlanet);
    }

    void Update()
    {
        reserveSlider.value = (float) MiningManager.Instance.CurrentPlanet.GetCurrentReserveRatio();
    }

    // Refreshes all mining UI for a given planet.
    public void RefreshPlanetUI(Planet planet)
    {
        if (planet == null) return;

        if (upgradeUI != null)
            upgradeUI.Refresh(planet);


        if (planetNameText) planetNameText.text = planet.planetName;

        UpdateProbeSlots(planet);
        UpdateRateDisplay(planet);
        UpdateGlobalRateDisplay();

    }

    // Updates the slot widgets to show occupied vs empty slots.
    public void UpdateProbeSlots(Planet planet)
    {
        int maxSlots = planet.GetEffectiveMaxProbeSlots();
        int occupied = planet.Probes.Count;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            bool show = i < maxSlots;
            slotUIs[i].gameObject.SetActive(show);

            if (!show)
                continue;

            if (i < occupied)
            {
                Probe probe = planet.Probes[i];
                slotUIs[i].Bind(probe, planet, probeUI);
            }
            else
            {
                slotUIs[i].ShowEmpty();
            }
        }

        
    }

    // Updates the glass/sec and cores/sec text for the current planet.
    public void UpdateRateDisplay(Planet planet)
    {
        ProbeOutput output = planet.GetAggregatedOutput();
        if (glassPerSecText) glassPerSecText.text = $"{output.glass:0.#}/s";
        if (coresPerSecText) coresPerSecText.text = $"{output.cores:0.#}/s";
    }

    public void UpdateGlobalRateDisplay()
    {
        if (MiningManager.Instance == null)
            return;

        float totalGlass = 0f;
        float totalCores = 0f;

        foreach (var planet in MiningManager.Instance.UnlockedPlanets)
        {
            if (planet == null)
                continue;

            ProbeOutput output = planet.GetAggregatedOutput();
            totalGlass += output.glass;
            totalCores += output.cores;
        }

        if (globalGlassPerSecText) globalGlassPerSecText.text = $"{totalGlass:0.#}/s";
        if (globalCoresPerSecText) globalCoresPerSecText.text = $"{totalCores:0.#}/s";
    }


    // Refreshes the entire mining UI (planet info + slots + rates + buy buttons).
    public void RefreshAllUI()
    {
        var planet = MiningManager.Instance.CurrentPlanet;
        RefreshPlanetUI(planet);

        // If you keep a list of buy buttons, refresh them here.
        foreach (var button in buyButtons)
            button.RefreshState();
    }

    public void RegisterBuyButton(ProbeBuyButton button)
    {
        if (!buyButtons.Contains(button))
            buyButtons.Add(button);
    }
}
