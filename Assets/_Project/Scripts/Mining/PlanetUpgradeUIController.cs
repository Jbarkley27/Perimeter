using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


/*
 * PlanetUpgradeUIController
 * -------------------------
 * Owns the upgrade grid and reroll button for the current planet.
 * Binds nodes, manages icons, and keeps affordability up to date.
 */
public class PlanetUpgradeUIController : MonoBehaviour
{
    [Header("Upgrade Grid")]
    public List<PlanetUpgradeNodeUI> upgradeNodes = new List<PlanetUpgradeNodeUI>();
    public PlanetUpgradeInfoPanel infoPanel;

    [Header("Reroll")]
    public Button rerollButton;
    public TMP_Text rerollCostText;
    public CanvasGroup rerollCanvasGroup;

    [Header("Icons")]
    public List<PlanetUpgradeIconEntry> iconEntries = new List<PlanetUpgradeIconEntry>();

    private readonly Dictionary<PlanetUpgradeId, Sprite> iconLookup = new Dictionary<PlanetUpgradeId, Sprite>();
    private Planet currentPlanet;

    [Header("Follow Mouse Settings")]
    public Vector3 mouseOffset = new Vector3(15f, -15f, 0f);
    public Canvas rootCanvas;
    private RectTransform infoPanelRect;

    [Header("Refund")]
    public Button refundButton;
    public CanvasGroup refundCanvasGroup;
    public TMP_Text refundHoverText;



    // Build icon lookup and hide the info panel on startup.
    private void Awake()
    {
        BuildIconLookup();

        if (infoPanel != null)
            infoPanel.Hide();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (infoPanel != null)
            infoPanelRect = infoPanel.GetComponent<RectTransform>();
    }

    // Refresh UI when this panel is enabled.
    private void OnEnable()
    {
        Refresh(MiningManager.Instance != null ? MiningManager.Instance.CurrentPlanet : null);
    }

    // Keep affordability and hover panel accurate as Glass changes.
    private void Update()
    {
        RefreshAffordability();

        if (infoPanel != null && infoPanel.gameObject.activeSelf)
            FollowMousePosition(GetCursorPosition());
    }


    // Binds the upgrade UI to the provided planet.
    public void Refresh(Planet planet)
    {
        if (planet != null)
            planet.EnsureStartingUpgrades();

        currentPlanet = planet;

        if (rerollCostText != null)
            rerollCostText.text = PlanetUpgradeTuning.RerollCost.ToString("0");

        if (upgradeNodes == null)
            return;

        // Clear if no planet.
        if (planet == null)
        {
            foreach (var node in upgradeNodes)
                node.Clear();
            return;
        }

        IReadOnlyList<PlanetUpgradeInstance> upgrades = planet.Upgrades;

        for (int i = 0; i < upgradeNodes.Count; i++)
        {
            if (i < upgrades.Count)
                upgradeNodes[i].Bind(planet, upgrades[i], this);
            else
                upgradeNodes[i].Clear();
        }

        RefreshRefundState();


        RefreshAffordability();
    }

    // Updates button interactability based on current Glass.
    public void RefreshAffordability()
    {
        if (currentPlanet == null)
            return;

        if (rerollButton != null)
        {
            bool canAffordReroll = GlassManager.Instance != null
                && GlassManager.Instance.CanAffordGlass(PlanetUpgradeTuning.RerollCost);

            rerollButton.interactable = canAffordReroll;

            if (rerollCanvasGroup != null)
                rerollCanvasGroup.alpha = canAffordReroll ? 1f : 0.4f;
        }

        foreach (var node in upgradeNodes)
            node.RefreshAffordability();

        // Keep hover info up-to-date if it's open.
        if (infoPanel != null && infoPanel.gameObject.activeSelf)
            infoPanel.Refresh();

        RefreshRefundState();

    }

    // Called by the reroll button.
    public void OnRerollClicked()
    {
        PunchButton(rerollButton);

        if (currentPlanet == null)
            return;

        if (currentPlanet.TryRerollUpgrades())
            Refresh(currentPlanet);
    }

    // Called by nodes/panels when an upgrade changes.
    public void NotifyUpgradeChanged()
    {
        Refresh(currentPlanet);
    }

    // Returns the icon assigned to a specific upgrade id.
    public Sprite GetIcon(PlanetUpgradeId id)
    {
        if (iconLookup.TryGetValue(id, out var sprite))
            return sprite;

        return null;
    }

    // Shows the info panel for a specific upgrade.
    public void ShowInfo(Planet planet, PlanetUpgradeInstance upgrade)
    {
        if (infoPanel == null)
            return;

        infoPanel.Show(planet, upgrade, this);
        FollowMousePosition(GetCursorPosition());
    }

    // Hides the info panel.
    public void HideInfo()
    {
        if (infoPanel == null)
            return;

        infoPanel.Hide();
    }


    // Builds a fast lookup from upgrade id to sprite icon.
    private void BuildIconLookup()
    {
        iconLookup.Clear();
        for (int i = 0; i < iconEntries.Count; i++)
        {
            var entry = iconEntries[i];
            if (!iconLookup.ContainsKey(entry.id))
                iconLookup.Add(entry.id, entry.icon);
        }
    }


    // Plays a punch scale animation on a button and restores scale on completion.
    private void PunchButton(Button button)
    {
        if (button == null)
            return;

        Transform target = button.transform;
        Vector3 original = target.localScale;

        target.DOKill();
        target.DOPunchScale(Vector3.one * 0.12f, 0.2f, 10, 1f)
            .OnComplete(() => target.localScale = original);
    }



    // Returns the current cursor position (WorldCursor if available, otherwise mouse).
    private Vector3 GetCursorPosition()
    {
        if (WorldCursor.instance != null)
            return WorldCursor.instance.GetCursorPosition();

        return Input.mousePosition;
    }

    // Moves the hover panel to the cursor with a configurable offset.
    private void FollowMousePosition(Vector3 mousePosition)
    {
        if (infoPanelRect == null)
            return;

        RectTransform parentRect = infoPanelRect.parent as RectTransform;
        if (parentRect == null)
            return;

        Camera cam = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            mousePosition,
            cam,
            out Vector2 localPoint
        );

        infoPanelRect.anchoredPosition = localPoint + (Vector2)mouseOffset;
    }



        // Returns the current refund amount for the active planet.
    private double GetRefundAmount()
    {
        if (currentPlanet == null)
            return 0;

        return currentPlanet.GetUpgradeRefundAmount();
    }

    // Updates refund button state and hover text.
    private void RefreshRefundState()
    {
        if (refundButton == null)
            return;

        double refundAmount = GetRefundAmount();
        bool canRefund = refundAmount > 0;

        refundButton.interactable = canRefund;

        if (refundCanvasGroup != null)
            refundCanvasGroup.alpha = canRefund ? 1f : 0.4f;

        if (refundHoverText != null)
            refundHoverText.text = $"Refund: {refundAmount:0}";

    }

    // Called by the refund button.
    public void OnRefundClicked()
    {
        PunchButton(refundButton);

        if (currentPlanet == null)
            return;

        if (currentPlanet.TryRefundAllUpgrades())
            Refresh(currentPlanet);
    }
}

[Serializable]
public struct PlanetUpgradeIconEntry
{
    public PlanetUpgradeId id;
    public Sprite icon;
}
