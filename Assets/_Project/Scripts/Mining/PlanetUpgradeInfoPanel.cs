using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * PlanetUpgradeInfoPanel
 * ----------------------
 * Hover panel that shows upgrade details and offers an upgrade button.
 */
public class PlanetUpgradeInfoPanel : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text levelText;
    public TMP_Text effectText;
    public TMP_Text costText;
    public Button upgradeButton;
    public CanvasGroup canvasGroup;

    private Planet currentPlanet;
    private PlanetUpgradeInstance currentUpgrade;
    private PlanetUpgradeUIController owner;

    public TMP_Text refundText;



    // Keeps the panel accurate while it's visible.
    private void Update()
    {
        if (currentPlanet != null && currentUpgrade != null)
            Refresh();
    }


    // Shows the panel for a specific upgrade.
    public void Show(Planet planet, PlanetUpgradeInstance upgrade, PlanetUpgradeUIController ownerController)
    {
        currentPlanet = planet;
        currentUpgrade = upgrade;
        owner = ownerController;

        gameObject.SetActive(true);
        Refresh();
    }

    // Hides the panel.
    public void Hide()
    {
        currentPlanet = null;
        currentUpgrade = null;
        owner = null;

        gameObject.SetActive(false);
    }

    // Updates all text and button state.
    public void Refresh()
    {
        if (currentPlanet == null || currentUpgrade == null)
            return;

        PlanetUpgradeDefinition def = PlanetUpgradeCatalog.Get(currentUpgrade.id);
        if (def == null)
            return;

        bool isMax = currentPlanet.IsUpgradeMaxed(currentUpgrade.id);
        double cost = currentPlanet.GetUpgradeCost(currentUpgrade.id);

        if (titleText != null)
            titleText.text = def.displayName;

        if (levelText != null)
        {
            levelText.text = isMax
                ? $"Level {currentUpgrade.level}/{def.maxLevel} (MAX)"
                : $"Level {currentUpgrade.level}/{def.maxLevel}";
        }

        if (effectText != null)
            effectText.text = def.GetEffectText(currentUpgrade.level);

        if (costText != null)
        {
            costText.gameObject.SetActive(true);
            costText.text = isMax ? "Upgrade: MAX" : $"Upgrade: {cost:0}";
        }

        if (upgradeButton != null)
            upgradeButton.interactable = !isMax;



        // Intentionally do not modify panel canvas group alpha.

        if (refundText != null)
        {
            double refundAmount = currentPlanet.GetUpgradeRefundAmount(currentUpgrade.id);
            refundText.text = $"Refund: {refundAmount:0}";
        }


    }

    // Called by the panel’s Upgrade button.
    // public void OnUpgradeClicked()
    // {
    //     if (currentPlanet == null || currentUpgrade == null)
    //         return;

    //     if (currentPlanet.TryUpgrade(currentUpgrade.id))
    //     {
    //         Refresh();
    //         if (owner != null)
    //             owner.NotifyUpgradeChanged();
    //     }
    // }
}
