using UnityEngine;

/*
 * ProbeUIController
 * -----------------
 * Shared hover panel + upgrade/refund actions for probes.
 */
public class ProbeUIController : MonoBehaviour
{
    [Header("Hover Panel")]
    public ProbeHoverInfoPanel infoPanel;
    public Vector3 mouseOffset = new Vector3(15f, -15f, 0f);
    public Canvas rootCanvas;
    private RectTransform infoPanelRect;

    private void Awake()
    {
        if (infoPanel != null)
            infoPanel.Hide();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (infoPanel != null)
            infoPanelRect = infoPanel.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (infoPanel != null && infoPanel.gameObject.activeSelf)
            FollowMousePosition(GetCursorPosition());
    }

    public void ShowInfo(Probe probe, Planet planet)
    {
        if (infoPanel == null || probe == null || planet == null)
            return;
        
        Debug.Log("ProbeUIController.ShowInfo called");


        double refundAmount = ProbeManager.Instance.GetProbeRefundAmount(probe, planet);
        string description = ProbeDescriptionLibrary.GetDescription(probe);
        Sprite icon = ProbeManager.Instance != null
            ? ProbeManager.Instance.GetProbeIcon(probe.Type)
            : null;

        string upgradeCostValue = "MAX";
        bool isMax = planet != null && probe != null && probe.Level >= planet.GetMaxProbeLevel();

        if (!isMax)
        {
            double cost = ProbeManager.Instance.GetUpgradeCost(probe, planet);
            upgradeCostValue = cost.ToString("0");
        }
        
        infoPanel.Show(probe, planet, icon, description, upgradeCostValue, refundAmount);


        FollowMousePosition(GetCursorPosition());
    }

    public void HideInfo()
    {
        if (infoPanel != null)
            infoPanel.Hide();
    }

        public bool TryUpgrade(Probe probe, Planet planet)
    {
        if (ProbeManager.Instance.UpgradeProbe(probe, planet))
        {
            MiningManager.Instance.miningUI.RefreshPlanetUI(planet);
            ShowInfo(probe, planet);
            return true;
        }
        return false;
    }

    public bool TryRefund(Probe probe, Planet planet)
    {
        if (ProbeManager.Instance.RefundProbe(probe, planet))
        {
            MiningManager.Instance.miningUI.RefreshPlanetUI(planet);
            HideInfo();
            return true;
        }
        return false;
    }


    private Vector3 GetCursorPosition()
    {
        if (WorldCursor.instance != null)
            return WorldCursor.instance.GetCursorPosition();

        return Input.mousePosition;
    }

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
}
