using System.Collections.Generic;
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
    private bool isHovering;

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

        bool hasActive = activeModifier != null;
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
        if (hoverPanelRoot == null)
            return;

        hoverPanelRoot.transform.position = mousePosition + mouseOffset;
    }

}
