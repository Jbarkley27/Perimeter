using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * PlanetUpgradeNodeUI
 * -------------------
 * One icon-only node in the upgrade grid.
 * Shows icon + progress slider. Full info is in the hover panel.
 */
public class PlanetUpgradeNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;
    public Button upgradeButton;
    public CanvasGroup canvasGroup;
    public Slider levelSlider;

    private Planet boundPlanet;
    private PlanetUpgradeInstance boundUpgrade;
    private PlanetUpgradeUIController owner;

    private PlanetUpgradeInstance lastUpgrade;
    private int lastLevel = -1;

    // Binds this node to a specific upgrade instance.
    public void Bind(Planet planet, PlanetUpgradeInstance upgrade, PlanetUpgradeUIController ownerController)
    {
        boundPlanet = planet;
        boundUpgrade = upgrade;
        owner = ownerController;

        bool upgradeChanged = boundUpgrade != lastUpgrade;
        lastUpgrade = boundUpgrade;

        if (upgradeChanged && boundUpgrade != null)
            lastLevel = boundUpgrade.level;

        gameObject.SetActive(true);
        Refresh();
    }

    // Clears the node when no upgrade is present.
    public void Clear()
    {
        boundPlanet = null;
        boundUpgrade = null;
        owner = null;
        lastUpgrade = null;
        lastLevel = -1;
        gameObject.SetActive(false);
    }

    // Updates icon + slider + affordability.
    public void Refresh()
    {
        if (boundPlanet == null || boundUpgrade == null)
            return;

        PlanetUpgradeDefinition def = PlanetUpgradeCatalog.Get(boundUpgrade.id);
        if (def == null)
            return;

        if (iconImage != null && owner != null)
        {
            iconImage.sprite = owner.GetIcon(boundUpgrade.id);
            iconImage.enabled = iconImage.sprite != null;
        }

        int newLevel = boundUpgrade.level;
        bool levelIncreased = lastLevel >= 0 && newLevel > lastLevel;

        if (levelSlider != null)
        {
            levelSlider.minValue = 0;
            levelSlider.maxValue = def.maxLevel;
            levelSlider.value = newLevel;
        }

        if (levelIncreased && levelSlider != null)
            PunchTransform(levelSlider.transform, 0.08f, 0.15f);

        lastLevel = newLevel;

        RefreshAffordability();
    }

    // Updates affordability and button state.
    public void RefreshAffordability()
    {
        if (boundPlanet == null || boundUpgrade == null)
            return;

        bool isMax = boundPlanet.IsUpgradeMaxed(boundUpgrade.id);
        bool canAfford = boundPlanet.CanAffordUpgrade(boundUpgrade.id);

        if (upgradeButton != null)
            upgradeButton.interactable = !isMax && canAfford;

        if (canvasGroup != null)
            canvasGroup.alpha = (!isMax && canAfford) ? 1f : 0.4f;
    }

    // Called by the node’s button.
    public void OnUpgradeClicked()
    {
        if (boundPlanet == null || boundUpgrade == null)
            return;

        if (boundPlanet.TryUpgrade(boundUpgrade.id))
        {
            PunchTransform(upgradeButton != null ? upgradeButton.transform : transform, 0.12f, 0.2f);
            Refresh();

            if (owner != null)
                owner.NotifyUpgradeChanged();
        }
    }

    // Hover: show info panel.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null && boundPlanet != null && boundUpgrade != null)
            owner.ShowInfo(boundPlanet, boundUpgrade);
    }

    // Hover: hide info panel.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (owner != null)
            owner.HideInfo();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (boundPlanet != null && boundUpgrade != null)
        {
            if (boundPlanet.TryRefundUpgrade(boundUpgrade.id))
            {
                PunchTransform(transform, 0.12f, 0.2f);
                Refresh();

                if (owner != null)
                    owner.NotifyUpgradeChanged();
            }
        }
    }


    // Punches a target transform and restores its scale on completion.
    private void PunchTransform(Transform target, float strength, float duration)
    {
        if (target == null)
            return;

        Vector3 original = target.localScale;
        target.DOKill();
        target.DOPunchScale(Vector3.one * strength, duration, 10, 1f)
            .OnComplete(() => target.localScale = original);
    }
}
