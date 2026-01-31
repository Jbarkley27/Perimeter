using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Manages Mining tab UI: planet name, probe slots, and rate display.
public class MiningUIController : MonoBehaviour
{
    public TMP_Text planetNameText;
    public TMP_Text glassPerSecText;
    public TMP_Text coresPerSecText;
    public List<ProbeSlotUI> slotUIs = new List<ProbeSlotUI>();

    // Refreshes all mining UI for a given planet.
    public void RefreshPlanetUI(Planet planet)
    {
        if (planet == null) return;

        if (planetNameText) planetNameText.text = planet.planetName;

        UpdateProbeSlots(planet);
        UpdateRateDisplay(planet);
    }

    // Updates the slot widgets to show occupied vs empty slots.
    public void UpdateProbeSlots(Planet planet)
    {
        int maxSlots = planet.MaxProbeSlots; // add this field to Planet
        int occupied = planet.Probes.Count;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            bool show = i < maxSlots;
            slotUIs[i].gameObject.SetActive(show);

            if (show)
                slotUIs[i].SetOccupied(i < occupied);

            Sprite icon = null;
            if (i < occupied)
                icon = ProbeManager.Instance.GetProbeIcon(planet.Probes[i].Type);

            slotUIs[i].SetOccupied(i < occupied, icon);

        }
        
    }

    // Updates the glass/sec and cores/sec text for the current planet.
    public void UpdateRateDisplay(Planet planet)
    {
        PlanetContext context = planet.GetProbeContext();
        float glass = 0f;
        float cores = 0f;

        foreach (var probe in planet.Probes)
        {
            ProbeOutput output = probe.GetOutput(context);
            glass += output.glass;
            cores += output.cores;
        }

        if (glassPerSecText) glassPerSecText.text = $"{glass:0.#}/s";
        if (coresPerSecText) coresPerSecText.text = $"{cores:0.#}/s";
    }
}
