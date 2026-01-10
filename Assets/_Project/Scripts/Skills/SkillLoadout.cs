using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    public List<SkillUISlot> uiSlots;
    public Transform uiSlotParent;

    void Awake()
    {
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
    }

    void Start()
    {
        RefreshHUD();
    }

    public void RefreshHUD()
    {
        Debug.Log("Refreshing Player HUD Skills");

        var equipped = RunManager.Instance.GetEquippedSkills();

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
    }
}
