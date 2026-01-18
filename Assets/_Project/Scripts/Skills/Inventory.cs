using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public Transform inventorySlotParent;
    public InventorySlot activeSlot;
    public List<SkillData> allSkills = new List<SkillData>();

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
    }


    public void SetSlotAsSelected(InventorySlot selectedSlot)
    {
        if (inventorySlots.Contains(selectedSlot) == false)
        {
            Debug.LogError("Inventory: Trying to select a slot that is not in the inventory.");
            return;
        }

        if (activeSlot != null)
        {
            activeSlot.SetAsInactiveSlot();
        }

        activeSlot = selectedSlot;
        foreach (var slot in inventorySlots)
        {
            slot.SetAsActiveSlot();
        }
    }
}