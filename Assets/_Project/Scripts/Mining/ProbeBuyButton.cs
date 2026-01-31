using UnityEngine;
using UnityEngine.UI;

// Handles purchase button behavior for a specific probe type.
public class ProbeBuyButtonUI : MonoBehaviour
{
    public ProbeType type;
    public Image iconImage;
    public Button button;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    // Refreshes the icon on enable (safe if ProbeManager is ready).
    private void OnEnable()
    {
        RefreshIcon();
    }

    void Update()
    {
        bool unlocked = ProbeManager.Instance.IsProbeUnlocked(type);
        button.gameObject.SetActive(unlocked);
    }

    // Pulls the correct icon for this probe type.
    public void RefreshIcon()
    {
        if (iconImage == null || ProbeManager.Instance == null)
            return;

        iconImage.sprite = ProbeManager.Instance.GetProbeIcon(type);
        iconImage.enabled = iconImage.sprite != null;
    }

    // Attempts to purchase a probe and refreshes UI if successful.
    public void OnClick()
    {
        var planet = MiningManager.Instance.CurrentPlanet;
        if (ProbeManager.Instance.PurchaseProbe(type, planet))
        {
            MiningManager.Instance.miningUI.RefreshPlanetUI(planet);
        }
    }
}
