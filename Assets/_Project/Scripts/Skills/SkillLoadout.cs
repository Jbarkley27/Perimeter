using System.Collections.Generic;
using UnityEngine;

public class SkillLoadout : MonoBehaviour
{
    [Header("UI Skill Slots")]
    public List<SkillUISlot> uiSlots;
    public Transform uiSlotParent;

    [Header("Loadout Drop Targets")]
    public List<LoadoutDropTarget> dropTargets;
    public Transform dropTargetParent;
    public float dropTargetDisabledScaleMultiplier = 0.3f;

    [Header("Equipped Skills (Displayed In HUD)")]
    public List<SkillData> equippedSkills = new List<SkillData>();
    public static SkillLoadout Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found a SkillLoadout object, destroying new one");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);



        // Auto-populate uiSlots from children if not set
        if (uiSlots == null || uiSlots.Count == 0)
        {
            uiSlots = new List<SkillUISlot>();
            foreach (Transform child in uiSlotParent)
            {
                if(child.gameObject.activeSelf == false)
                    continue;
                
                var skillUI = child.GetComponent<SkillUISlot>();
                if (skillUI != null)
                {
                    uiSlots.Add(skillUI);
                }
            }
        }

        if (dropTargets == null || dropTargets.Count == 0)
        {
            dropTargets = new List<LoadoutDropTarget>();
            if (dropTargetParent != null)
            {
                foreach (Transform child in dropTargetParent)
                {
                    var target = child.GetComponentInChildren<LoadoutDropTarget>(true);
                    if (target != null)
                        dropTargets.Add(target);
                }
            }
        }
    }

    void Start()
    {
        RefreshHUD();
    }

    public void EquipSkill(SkillData skill)
    {
        if (!equippedSkills.Contains(skill))
        {
            equippedSkills.Add(skill);
            RefreshHUD();
        }
    }

    public void UnequipSkill(SkillData skill)
    {
        if (equippedSkills.Contains(skill))
        {
            equippedSkills.Remove(skill);
            RefreshHUD();
        }
    }

    public void RefreshHUD()
    {
        Debug.Log("Refreshing Player HUD Skills");

        EnforceMaxLoadoutSlots();

        var equipped = equippedSkills;

        // Lets disable all slots first
        foreach (var slot in uiSlots)
        {
            slot.ClearSlot();
            slot.gameObject.SetActive(false);
        }

        // Now enable and set up only the equipped ones
        for (int i = 0; i < equipped.Count && i < uiSlots.Count; i++)
        {
            uiSlots[i].gameObject.SetActive(true);
            uiSlots[i].Init(equipped[i]);
        }

        RefreshDropTargets();
    }

    public bool IsSkillEquipped(SkillData skill)
    {
        return equippedSkills.Contains(skill);
    }


    public void RefreshSlotElementColors()
    {
        foreach (var slot in uiSlots)
        {
            if (slot != null && slot.currentSkill != null && slot.gameObject.activeSelf)
                slot.RefreshElementColor();
        }
    }

    private void RefreshDropTargets()
    {
        if (dropTargets == null || dropTargets.Count == 0)
            return;

        List<SkillDraggable> occupied = new List<SkillDraggable>();
        for (int i = 0; i < dropTargets.Count; i++)
        {
            LoadoutDropTarget target = dropTargets[i];
            if (target != null && target.OccupiedItem != null)
                occupied.Add(target.OccupiedItem);
        }

        // Repack to the left so there are no gaps.
        for (int i = 0; i < dropTargets.Count; i++)
        {
            LoadoutDropTarget target = dropTargets[i];
            if (target == null)
                continue;

            if (i < occupied.Count)
            {
                SkillDraggable draggable = occupied[i];
                if (draggable != null && draggable.currentSlot != target)
                    draggable.SnapToSlot(target, false);
            }
            else if (target.OccupiedItem != null)
            {
                target.ClearSlot();
            }
        }

        int maxSlots = dropTargets.Count;
        if (StatsManager.Instance != null)
            maxSlots = Mathf.Clamp(StatsManager.Instance.GetMaxLoadoutSlots(), 1, dropTargets.Count);

        int activeIndex = occupied.Count < maxSlots ? occupied.Count : -1;

        for (int i = 0; i < dropTargets.Count; i++)
        {
            LoadoutDropTarget target = dropTargets[i];
            if (target == null)
                continue;

            bool isEnabledSlot = i < maxSlots;
            bool isActive = isEnabledSlot && i == activeIndex;
            bool isDisabled = !isEnabledSlot || (activeIndex >= 0 && i > activeIndex);
            float scaleMult = isDisabled ? dropTargetDisabledScaleMultiplier : 1f;
            if (target.hoverCanvasGroup != null)
                target.hoverCanvasGroup.alpha = isActive ? 1f : 0.4f;
            target.SetSlotState(i, isActive, isDisabled, isEnabledSlot, scaleMult);
        }
    }

    private void EnforceMaxLoadoutSlots()
    {
        if (dropTargets == null || dropTargets.Count == 0)
            return;

        int maxSlots = dropTargets.Count;
        if (StatsManager.Instance != null)
            maxSlots = Mathf.Clamp(StatsManager.Instance.GetMaxLoadoutSlots(), 1, dropTargets.Count);

        for (int i = maxSlots; i < dropTargets.Count; i++)
        {
            LoadoutDropTarget target = dropTargets[i];
            if (target == null || target.OccupiedItem == null)
                continue;

            SkillDraggable draggable = target.OccupiedItem;
            target.ClearSlot();

            if (draggable != null)
            {
                if (draggable.skillData != null && equippedSkills.Contains(draggable.skillData))
                    equippedSkills.Remove(draggable.skillData);

                draggable.Snapping = false;
                draggable.ReturnToOriginalPosition();
            }
        }

        // Safety: clamp equipped list if it somehow exceeds max slots.
        while (equippedSkills.Count > maxSlots)
            equippedSkills.RemoveAt(equippedSkills.Count - 1);
    }
}
