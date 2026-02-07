using System.Collections.Generic;
using UnityEngine;

/*
 * SectorCompassUIController
 * -------------------------
 * Binds compass choices to direction nodes and drives the hover panel.
 */
public class SectorCompassUIController : MonoBehaviour
{
    [Header("Nodes")]
    public List<SectorCompassNodeUI> nodes = new List<SectorCompassNodeUI>();

    [Header("Rewards Box")]
    public SectorRewardsBoxUI rewardsBox;

    private SectorCompassChoice? hoveredChoice;

    [Header("Hover Info")]
    public SectorCompassInfoPanel infoPanel;
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

    // Binds current pending choices from SectorManager.
    public void RefreshFromPending()
    {
        if (SectorManager.Instance == null)
            return;

        IReadOnlyList<SectorCompassChoice> choices = SectorManager.Instance.PendingCompassChoices;
        BindChoices(choices);
    }

    // Binds a set of choices to the compass UI.
    public void BindChoices(IReadOnlyList<SectorCompassChoice> choices)
    {
        if (nodes == null)
            return;

        // Clear all nodes first.
        for (int i = 0; i < nodes.Count; i++)
            nodes[i].Clear();

        if (choices == null)
            return;

        for (int i = 0; i < nodes.Count; i++)
        {
            SectorCompassNodeUI node = nodes[i];
            if (node == null)
                continue;

            bool hasChoice = TryGetChoiceForDirection(node.direction, choices, out SectorCompassChoice choice);
            if (hasChoice)
                node.Bind(choice, this);
        }

        HideInfo();
    }

    // Hover show.
    public void ShowInfo(SectorCompassChoice choice)
    {
        hoveredChoice = choice;

        if (infoPanel == null)
            return;

        List<SectorRewardEntry> rewards = BuildRewardPreview(choice);
        infoPanel.Show(choice, rewards);
        FollowMousePosition(GetCursorPosition());
    }

    // Hover hide.
    public void HideInfo()
    {
        hoveredChoice = null;

        if (infoPanel != null)
            infoPanel.Hide();
    }

    // Click select.
    public void SelectChoice(SectorCompassChoice choice)
    {
        // Route through RunManager so it can restart the run + apply visuals.
        if (RunManager.Instance != null)
        {
            RunManager.Instance.OnCompassChoiceSelected(choice);
            return;
        }

        if (SectorManager.Instance != null)
            SectorManager.Instance.SelectCompassChoice(choice);
    }


    // Builds the reward list shown on hover (sector base + modifier rewards).
    private List<SectorRewardEntry> BuildRewardPreview(SectorCompassChoice choice)
    {
        List<SectorRewardEntry> rewards = new List<SectorRewardEntry>();

        Sector current = SectorManager.Instance != null ? SectorManager.Instance.GetCurrentSector() : null;
        if (current != null && current.baseRewards != null)
            rewards.AddRange(current.baseRewards);

        if (choice.modifier != null && choice.modifier.rewards != null)
            rewards.AddRange(choice.modifier.rewards);

        return rewards;
    }

    private bool TryGetChoiceForDirection(SectorDirection direction, IReadOnlyList<SectorCompassChoice> choices, out SectorCompassChoice choice)
    {
        for (int i = 0; i < choices.Count; i++)
        {
            if (choices[i].direction == direction)
            {
                choice = choices[i];
                return true;
            }
        }

        choice = default;
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
