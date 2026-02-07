using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * ReserveSliderTooltip
 * --------------------
 * Shows a small hover tooltip for the reserve slider.
 * Displays current reserves and explains their impact on mining efficiency.
 */
public class ReserveSliderTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip")]
    public GameObject tooltipRoot;
    public TMP_Text titleText;
    public TMP_Text amountText;
    public TMP_Text descriptionText;

    [Header("Follow Mouse")]
    public Vector3 mouseOffset = new Vector3(15f, -15f, 0f);
    public Canvas rootCanvas;

    private bool isHovering;
    private RectTransform tooltipRect;

    // Hide the tooltip on startup.
    private void Awake()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (tooltipRoot != null)
            tooltipRect = tooltipRoot.GetComponent<RectTransform>();
    }

    // Keep the tooltip updated while hovering.
    private void Update()
    {
        if (!isHovering)
            return;

        UpdateTooltip();
        FollowMousePosition(GetCursorPosition());
    }

    // Show the tooltip when the slider is hovered.
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (tooltipRoot != null)
            tooltipRoot.SetActive(true);

        UpdateTooltip();
        FollowMousePosition(GetCursorPosition());
    }

    // Hide the tooltip when the slider is no longer hovered.
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }

    // Builds the tooltip text based on the current planet reserves.
    private void UpdateTooltip()
    {
        Planet planet = MiningManager.Instance != null ? MiningManager.Instance.CurrentPlanet : null;

        if (titleText != null)
            titleText.text = planet != null ? $"{planet.planetName} Reserves" : "Reserves";

        if (amountText != null)
        {
            if (planet == null)
            {
                amountText.text = "Glass: -";
            }
            else
            {
                PlanetContext context = planet.GetProbeContext();
                amountText.text = $"Glass: {context.currentReserves:0} / {context.maxReserves:0}";
            }
        }

        if (descriptionText != null)
            descriptionText.text = "Reserves fuel mining output. Lower reserves reduce probe efficiency.";
    }

    // Returns the current cursor position (WorldCursor if available, otherwise mouse).
    private Vector3 GetCursorPosition()
    {
        if (WorldCursor.instance != null)
            return WorldCursor.instance.GetCursorPosition();

        return Input.mousePosition;
    }

    // Moves the tooltip to the cursor with the configured offset.
    private void FollowMousePosition(Vector3 mousePosition)
    {
        if (tooltipRect == null)
            return;

        RectTransform parentRect = tooltipRect.parent as RectTransform;
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

        tooltipRect.anchoredPosition = localPoint + (Vector2)mouseOffset;
    }
}
