using System.Collections.Generic;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    public static SkillTree Instance { get; private set; }

    [Header("All Skills The Player Has Unlocked")]
    // public List<SkillData> unlockedSkills = new List<SkillData>();

    [Header("Equipped Skills (Displayed In HUD)")]
    public List<SkillData> equippedSkills = new List<SkillData>();

    [Header("Max Equipped At Once")]
    public int maxEquipped = 3;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<SkillData> GetEquippedSkills()
    {
        return equippedSkills;
    }
}
