using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class ConsoleUIManager : MonoBehaviour
{
    public enum ConsoleUIScreenState
    {
        MINING = 0,
        SKILL_TREE = 1,
        PRESTIGE = 2
    }

    public ConsoleUIScreenState CurrentScreenState = ConsoleUIScreenState.SKILL_TREE;

    public static ConsoleUIManager Instance { get; private set; }

    public Image consoleBackgroundImage;
    public GameObject consoleUIRoot;
    public CanvasGroup consoleCanvasGroup;
    public bool OpeningConsole = false;
    public GameObject HUDRoot;
    public GameObject SignalUIRoot;


    [Header("Mining Screen UI Elements")]
    public GameObject miningScreenUIRoot;
    public Image miningNavElementActiveIndicator;
    

    [Header("Skill Tree Screen UI Elements")]
    public GameObject skillTreeScreenUIRoot;
    public Image skillTreeNavElementActiveIndicator;


    [Header("Prestige Screen UI Elements")]
    public GameObject prestigeScreenUIRoot;
    public Image prestigeNavElementActiveIndicator;


    void Awake()
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
        UpdateScreenUI();
    }


    public void SetScreenState(int newState)
    {
        CurrentScreenState = (ConsoleUIScreenState)newState;
        UpdateScreenUI();
    }


    public void UpdateScreenUI()
    {
        miningScreenUIRoot.SetActive(CurrentScreenState == ConsoleUIScreenState.MINING);
        skillTreeScreenUIRoot.SetActive(CurrentScreenState == ConsoleUIScreenState.SKILL_TREE);
        prestigeScreenUIRoot.SetActive(CurrentScreenState == ConsoleUIScreenState.PRESTIGE);

        // Update active screen indicator
        switch (CurrentScreenState)
        {
            case ConsoleUIScreenState.MINING:
                miningNavElementActiveIndicator.enabled = true;
                skillTreeNavElementActiveIndicator.enabled = false;
                prestigeNavElementActiveIndicator.enabled = false;
                consoleBackgroundImage.enabled = false;
                break;

            case ConsoleUIScreenState.SKILL_TREE:
                miningNavElementActiveIndicator.enabled = false;
                skillTreeNavElementActiveIndicator.enabled = true;
                prestigeNavElementActiveIndicator.enabled = false;
                SkillTreeUIManager.Instance.CenterUIOnScreen();
                consoleBackgroundImage.enabled = true;
                break;

            case ConsoleUIScreenState.PRESTIGE:
                miningNavElementActiveIndicator.enabled = false;
                skillTreeNavElementActiveIndicator.enabled = false;
                prestigeNavElementActiveIndicator.enabled = true;
                consoleBackgroundImage.enabled = true;
                break;
        }
    }


    public void OpenConsole()
    {
        if (OpeningConsole)
            return; 

        HUDRoot.SetActive(false);
        SignalUIRoot.SetActive(false);
        OpeningConsole = true;
        RunManager.Instance.HideEndRunScreen();
        consoleCanvasGroup.alpha = 0;
        consoleUIRoot.SetActive(true);

        consoleCanvasGroup.DOFade(1, 0.2f).OnComplete(() =>
        {
            OpeningConsole = false;
        });

        UpdateScreenUI();
    }


    public void CloseConsole()
    {
        consoleCanvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            consoleUIRoot.SetActive(false);
        });
        HUDRoot.SetActive(true);
        SignalUIRoot.SetActive(true);
    }
}
