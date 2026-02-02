using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * ProbeHoverInfoPanel
 * -------------------
 * Shared hover panel for probes. Display-only; upgrades are done via node clicks.
 */
public class ProbeHoverInfoPanel : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text levelText;
    public TMP_Text descriptionText;
    public TMP_Text refundText;
    public Image iconImage;
    public Button refundButton;
    public Button upgradeButton;
    public CanvasGroup canvasGroup;
    public TMP_Text upgradeCostText;


    private Probe boundProbe;
    private Planet boundPlanet;

    // Shows this panel for the given probe.
    public void Show(Probe probe, Planet planet, Sprite icon, string description, string upgradeCostTextValue, double refundAmount)
    {
        boundProbe = probe;
        boundPlanet = planet;

        gameObject.SetActive(true);

        if (titleText != null)
            titleText.text = probe != null ? probe.Type.ToString() : "Probe";

        if (levelText != null)
            levelText.text = probe != null ? $"Level {probe.Level}" : "Level 0";

        bool isMax = planet != null && probe != null && probe.Level >= planet.GetMaxProbeLevel();
        if (upgradeCostText != null)
            upgradeCostText.gameObject.SetActive(true);

        if (upgradeButton == null && upgradeCostText != null)
            upgradeButton = upgradeCostText.GetComponentInParent<Button>();

        if (upgradeButton != null)
            upgradeButton.interactable = !isMax;


        if (descriptionText != null)
            descriptionText.text = description;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        // Display-only; refund happens via right-click.
        if (refundButton != null)
            refundButton.interactable = false;

        // Intentionally do not modify panel canvas group alpha.

        if (upgradeCostText != null)
            upgradeCostText.text = isMax ? "Upgrade: MAX" : $"Upgrade: {upgradeCostTextValue}";

        if (refundText != null)
            refundText.text = $"Refund: {refundAmount:0}";


    }

    // Hides the panel.
    public void Hide()
    {
        boundProbe = null;
        boundPlanet = null;
        gameObject.SetActive(false);
    }
}
