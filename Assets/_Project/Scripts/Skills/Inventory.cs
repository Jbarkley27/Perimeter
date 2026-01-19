using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public Transform inventorySlotParent;
    public InventorySlot activeSlot;
    public List<SkillData> allSkills = new List<SkillData>();
    public List<SkillData> startingSkills = new List<SkillData>();
    public CanvasGroup skillTreeRoot;
    public TMP_Text activeSkillNameText;
    public TMP_Text SkillCountText;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        SetupInventorySlots();
    }


    void Update()
    {
        skillTreeRoot.alpha = (activeSlot != null) ? 1 : 0;
        activeSkillNameText.text = (activeSlot != null && activeSlot.skillData != null) ? activeSlot.skillData.skillName : "No Skill Selected";
        SkillCountText.text = $"{GetUnlockedSkillCount()} / {allSkills.Count} Skills Unlocked";
    }


    public int GetUnlockedSkillCount()
    {
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.slotState == InventorySlot.SlotState.Unlocked)
            {
                count++;
            }
        }
        return count;
    }


    public void SetupInventorySlots()
    {
        inventorySlots.Clear();
        foreach (Transform child in inventorySlotParent)
        {
            InventorySlot slot = child.GetComponent<InventorySlot>();
            if (slot != null)
            {
                inventorySlots.Add(slot);
            }
        }

        // Add The skilldatas to the slots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < allSkills.Count)
            {
                inventorySlots[i].SetupSkillData(allSkills[i]);
            }
            else
            {
                inventorySlots[i].gameObject.SetActive(false);
            }
        }

        // Unlock starting skills
        foreach (var startingSkill in startingSkills)
        {
            Debug.Log($"Checking for starting skill to unlock: {startingSkill.skillName}");
            foreach (var slot in inventorySlots)
            {
                if (slot.skillData.skillName == startingSkill.skillName)
                {
                    Debug.Log($"Unlocking starting skill: {startingSkill.skillName}");
                    slot.SetSlotState(InventorySlot.SlotState.Unlocked);
                    break;
                }   
            }
        }


        SortInventorySlotByUnlocked();
    }


    public void SetSlotAsSelected(InventorySlot selectedSlot)
    {
        if (inventorySlots.Contains(selectedSlot) == false)
        {
            Debug.LogError("Inventory: Trying to select a slot that is not in the inventory.");
            return;
        }

        // if (activeSlot != null)
        // {
        //     activeSlot.SetAsInactiveSlot();
        // }

        activeSlot = selectedSlot;
        foreach (var slot in inventorySlots)
        {
            if (slot != selectedSlot)
                slot.SetAsInactiveSlot();
            else
                slot.SetAsActiveSlot();
        }

        skillTreeRoot.alpha = 0;
        // skillTreeActiveSkillSelectedUI.SetActive(true);
    }

    public bool IsActiveSkill(InventorySlot slot)
    {
        if (activeSlot == null || activeSlot.skillData == null)
            return false;

        return activeSlot.skillData.skillName == slot.skillData.skillName;
    }


    public void SortInventorySlotByUnlocked()
    {
        inventorySlots.Sort((a, b) =>
        {
            if (a.slotState == InventorySlot.SlotState.Unlocked && b.slotState != InventorySlot.SlotState.Unlocked)
                return -1;
            if (a.slotState != InventorySlot.SlotState.Unlocked && b.slotState == InventorySlot.SlotState.Unlocked)
                return 1;
            return 0;
        });

        // Reassign the slots in the hierarchy
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].transform.SetSiblingIndex(i);
        }
    }
}