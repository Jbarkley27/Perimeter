using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("All Skills The Player Has Unlocked")]
    // public List<SkillData> unlockedSkills = new List<SkillData>();

    [Header("Equipped Skills (Displayed In HUD)")]
    public List<SkillData> equippedSkills = new List<SkillData>();

    [Header("Max Equipped At Once")]
    public int maxEquipped = 3;


    [Header("Screens")]
    public GameObject endRunScreen;
    public CanvasGroup endRunCanvasGroup;
    public GameObject skillTreeScreen;
    public CanvasGroup skillTreeCanvasGroup;

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


    public void ShowEndRunScreen()
    {
        Debug.Log("Opening End Run Screen");
        endRunScreen.SetActive(true);
        endRunCanvasGroup.DOFade(1, 0.25f);
    }


    public void HideEndRunScreen()
    {
        if (endRunScreen.activeSelf == false)
            return;

        endRunCanvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            endRunScreen.SetActive(false);
        });
    }


    // public void OpenSkillTree()
    // {
    //     endRunCanvasGroup.DOFade(0, 0.2f).OnComplete(() =>
    //     {
    //         endRunScreen.SetActive(false);
    //     });
        
    //     skillTreeScreen.SetActive(true);
    //     skillTreeCanvasGroup.DOFade(1, 0.25f);
    // }


    // public void CloseSkillTree()
    // {
    //     skillTreeCanvasGroup.DOFade(0, 0.2f).OnComplete(() =>
    //     {
    //         skillTreeScreen.SetActive(false);
    //     });
    // }
}
