using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System.Collections;

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
    public CanvasGroup consoleTransitionScreen;
    public CanvasGroup consoleContentGroup;


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
        // Start with Skill Tree Screen
        CurrentScreenState = ConsoleUIScreenState.SKILL_TREE;
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
                // SkillTreeUIManager.Instance.CenterUIOnScreen();
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


    public void InitiateConsole()
    {
        if (OpeningConsole)
            return;

        OpeningConsole = true;

        StartCoroutine(OpenConsole());
    }



    public float consoleOpenDelay = 0.1f;
    public float transitionScreenOpenEndScale = 10f;

    public IEnumerator OpenConsole()
    {

        // Prepare Transition Screen
        consoleTransitionScreen.gameObject.SetActive(true);
        consoleTransitionScreen.gameObject.transform.localScale = Vector3.zero;

        consoleContentGroup.gameObject.SetActive(true);

        consoleContentGroup.DOFade(1, 0.15f);
        
        consoleCanvasGroup.gameObject.transform.DOShakePosition(0.3f, 0.5f, 10, 90, false)
            .OnComplete(() =>
            {
                consoleCanvasGroup.gameObject.transform.localPosition = Vector3.zero;
            });

        consoleTransitionScreen.DOFade(1, 0.2f);

        // Scale up transition screen
        consoleTransitionScreen.gameObject.transform.DOScale(Vector3.one * transitionScreenOpenEndScale,
             0.2f)
            .SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(0.1f + consoleOpenDelay);


        // Scale Back Down
        consoleContentGroup.DOFade(0, 0.1f);
        consoleTransitionScreen.gameObject.transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                consoleTransitionScreen.gameObject.transform.localScale = Vector3.one;
                consoleCanvasGroup.DOFade(1, 0.2f).OnComplete(() =>
                {
                    OpeningConsole = false;
                });

                consoleTransitionScreen.alpha = 0;
                consoleTransitionScreen.gameObject.SetActive(false);
                UpdateScreenUI();
            });


        // Background Stuff while the Transition screen is up
        HUDRoot.SetActive(false);
        SignalUIRoot.SetActive(false);
        RunManager.Instance.HideEndRunScreen();
        consoleCanvasGroup.alpha = 0;
        consoleUIRoot.SetActive(true);
    }


    public void CloseConsole()
    {
        consoleCanvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            consoleUIRoot.SetActive(false);
        });


        consoleContentGroup.alpha = 0;
        consoleTransitionScreen.alpha = 0;
        consoleContentGroup.gameObject.SetActive(false);
        consoleTransitionScreen.gameObject.SetActive(false);
        HUDRoot.SetActive(true);
        SignalUIRoot.SetActive(true);
        miningScreenUIRoot.SetActive(false);
        skillTreeScreenUIRoot.SetActive(false);
        prestigeScreenUIRoot.SetActive(false);
    }
}
