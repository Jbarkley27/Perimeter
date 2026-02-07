using UnityEngine;

/*
 * ProbePurchaseUIController
 * -------------------------
 * Drives the probe purchase hover panel and follows the cursor.
 */
public class ProbePurchaseUIController : MonoBehaviour
{
    [Header("Hover Panel")]
    public ProbePurchaseHoverPanel infoPanel;
    public Vector3 mouseOffset = new Vector3(15f, -15f, 0f);
    public Canvas rootCanvas;
    private RectTransform infoPanelRect;

    private ProbeType currentType;
    private Planet currentPlanet;
    private bool hasActive;

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

    // Shows the panel for a given probe type + planet.
    public void ShowInfo(ProbeType type, Planet planet)
    {
        currentType = type;
        currentPlanet = planet;
        hasActive = true;

        string name = type.ToString();
        string description = ProbeDescriptionLibrary.GetDescription(type);
        string costText = BuildCostText(type, planet);

        if (infoPanel != null)
            infoPanel.Show(name, description, costText);

        FollowMousePosition(GetCursorPosition());
    }

    // Hides the panel.
    public void HideInfo()
    {
        hasActive = false;

        if (infoPanel != null)
            infoPanel.Hide();
    }

    // Builds the display text for the Deploy cost.
    private string BuildCostText(ProbeType type, Planet planet)
    {
        if (planet == null || ProbeManager.Instance == null)
            return "Deploy: -";

        // Slots full = maxed for this planet.
        if (!planet.CanAddProbe())
            return "Probe Capacity Full";

        if (planet.CanUsePredictiveLogisticsFreeProbe())
            return "Deploy: Free";

        double cost = ProbeManager.Instance.GetBuyCost(type, planet);
        return $"Deploy: {cost:0}";
    }

    // Gets the cursor position (WorldCursor if available).
    private Vector3 GetCursorPosition()
    {
        if (WorldCursor.instance != null)
            return WorldCursor.instance.GetCursorPosition();

        return Input.mousePosition;
    }

    // Moves the panel to the cursor with offset.
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
