using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * SectorActiveModifierUI
 * ----------------------
 * Shows a small icon when a sector modifier is active.
 * Hover reveals a panel with modifier details and rewards.
 */
public class SectorActiveModifierUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Icon")]
    public Image iconImage;
    public GameObject iconRoot;

    [Header("Hover Panel")]
    public GameObject hoverPanelRoot;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text rarityText;
    public SectorRewardListUI rewardList;
    private SectorModifierDefinition activeModifier;


    [Header("Follow Mouse")]
    public Vector3 mouseOffset = new Vector3(15f, -15f, 0f);
    public Canvas rootCanvas;
    private bool isHovering;
    private RectTransform hoverPanelRect;

    private void Awake()
    {
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (hoverPanelRoot != null)
            hoverPanelRect = hoverPanelRoot.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!isHovering || hoverPanelRoot == null || !hoverPanelRoot.activeSelf)
            return;

        FollowMousePosition(GetCursorPosition());
    }


    // Refresh icon + hover content from SectorManager.
    public void Refresh()
    {
        activeModifier = SectorManager.Instance != null ? SectorManager.Instance.ActiveModifier : null;

        // Hide if no modifier or it's Hold Course
        bool hasActive = activeModifier != null && !activeModifier.isHoldCourse;
        if (iconRoot != null)
            iconRoot.SetActive(hasActive);

        if (!hasActive)
        {
            HideHover();
            return;
        }

        gameObject.transform.DOPunchScale(Vector3.one * .3f, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // reset scale to ensure no drift
                gameObject.transform.localScale = Vector3.one;
            });

        hasActive = activeModifier != null;
        if (iconRoot != null)
            iconRoot.SetActive(hasActive);

        if (!hasActive)
        {
            HideHover();
            return;
        }

        if (nameText != null)
            nameText.text = activeModifier.displayName;

        if (descriptionText != null)
            Debug.Log($"Setting description text to: {activeModifier.description}");
            descriptionText.text = activeModifier.description;

        if (rarityText != null)
            rarityText.text = activeModifier.rarity.ToString();

        if (rewardList != null)
            rewardList.SetRewards(activeModifier.rewards);
    }

     public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeModifier == null)
            return;

        isHovering = true;

        if (hoverPanelRoot != null)
            hoverPanelRoot.SetActive(true);

        FollowMousePosition(GetCursorPosition());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        HideHover();
    }


    private void HideHover()
    {
        if (hoverPanelRoot != null)
            hoverPanelRoot.SetActive(false);
    }


    private Vector3 GetCursorPosition()
    {
        if (WorldCursor.instance != null)
            return WorldCursor.instance.GetCursorPosition();

        return Input.mousePosition;
    }

    private void FollowMousePosition(Vector3 mousePosition)
    {
        if (hoverPanelRect == null)
            return;

        RectTransform parentRect = hoverPanelRect.parent as RectTransform;
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

        hoverPanelRect.anchoredPosition = localPoint + (Vector2)mouseOffset;
    }

}
