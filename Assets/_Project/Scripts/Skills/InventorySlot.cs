using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SlotState slotState = SlotState.Locked;
    public CanvasGroup canvasGroup;
    public SkillData skillData;
    public Image skillIconImage;
    public CanvasGroup hoveredHighlight;
    public bool isSelected = false;

    public enum SlotState
    {
        Unlocked,
        Locked,
        Empty,
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Inventory.Instance.IsActiveSkill(this))
            return;


        if (slotState == SlotState.Unlocked)
        {
            Inventory.Instance.SetSlotAsSelected(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("Pointer Entered Slot");
        if (slotState == SlotState.Unlocked && !isSelected)
        {
            hoveredHighlight.alpha = .6f;
            hoveredHighlight.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slotState == SlotState.Unlocked && !isSelected)
        {
            hoveredHighlight.alpha = 0f;
            hoveredHighlight.gameObject.SetActive(false);
        }
    }

    public void SetSlotState(SlotState newState)
    {
        slotState = newState;
        // Update visual representation based on state
        switch (slotState)
        {
            case SlotState.Unlocked:
                canvasGroup.alpha = .8f;
                break;
            case SlotState.Locked:
                canvasGroup.alpha = 0.3f;
                break;
        }
    }

    public void SetAsActiveSlot()
    {
        // Highlight this slot as active
        isSelected = true;
        hoveredHighlight.alpha = 1f;
        hoveredHighlight.gameObject.SetActive(true);
    }


    public void SetAsInactiveSlot()
    {
        // Remove highlight
        isSelected = false;
        hoveredHighlight.alpha = 0f;
        hoveredHighlight.gameObject.SetActive(false);
    }


    public void SetupSkillData(SkillData newSkillData)
    {
        // if null, set to empty state
        if (newSkillData == null)
        {
            SetSlotState(SlotState.Empty);
            skillIconImage.sprite = null;
            skillData = null;
            return;
        }

        // default to locked for now
        SetSlotState(SlotState.Locked);

        hoveredHighlight.gameObject.SetActive(false);

        skillData = newSkillData;
        if (skillIconImage != null && skillData != null)
        {
            skillIconImage.sprite = skillData.icon;
        }
    }
}