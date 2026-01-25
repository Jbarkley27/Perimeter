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

    [Header("General Console UI Elements")]
    public ConsoleUIScreenState CurrentScreenState = ConsoleUIScreenState.SKILL_TREE;

    public static ConsoleUIManager Instance { get; private set; }

    public Image consoleBackgroundImage;
    public GameObject consoleUIRoot;
    public CanvasGroup consoleCanvasGroup;
    public bool OpeningConsole = false;
    public GameObject HUDRoot;
    public GameObject DeltaBarRoot;
    public CanvasGroup consoleTransitionScreen;
    public float consoleOpenDelay = 0.1f;
    public float transitionScreenOpenEndScale = 10f;


    [Header("Mining Screen UI Elements")]
    public GameObject miningScreenUIRoot;
    public Image miningNavElementActiveIndicator;
    

    [Header("Skill Tree Screen UI Elements")]
    public GameObject skillTreeScreenUIRoot;
    public Image skillTreeNavElementActiveIndicator;


    [Header("Prestige Screen UI Elements")]
    public GameObject prestigeScreenUIRoot;
    public Image prestigeNavElementActiveIndicator;


    [Header("Skill Tree Ring UI Elements")]
    public Transform skillTreeRingRoot;


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

    public IEnumerator AnimateSkillTreeRing()
    {
        // small delay before starting animation
        yield return new WaitForSeconds(0.2f);

        // loop through root children and turn their canvas groups on and off in sequence
        foreach (Transform child in skillTreeRingRoot)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0;
                cg.DOFade(1, 0.25f).SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(0.15f);
            }
        }
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
                StartCoroutine(AnimateSkillTreeRing());
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




    public IEnumerator OpenConsole()
    {
        // Prepare Transition Screen Animation

        // Set initial states
        consoleTransitionScreen.gameObject.transform.localScale = Vector3.zero;
        consoleTransitionScreen.gameObject.SetActive(true);

        // Animate In Transition Screen
        consoleTransitionScreen.DOFade(1, 0.2f).SetEase(Ease.OutQuad);

        // Scale up transition screen
        consoleTransitionScreen.gameObject.transform.DOScale(Vector3.one * transitionScreenOpenEndScale,
             0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Background Stuff while the Transition screen is up
                HUDRoot.SetActive(false);
                DeltaBarRoot.SetActive(false);
                RunManager.Instance.HideEndRunScreen();
                consoleCanvasGroup.alpha = 0;
                consoleUIRoot.SetActive(true);
            });



        // Wait a moment while background stuff happens
        yield return new WaitForSeconds(0.1f + consoleOpenDelay);
  

    

        // Scale Back Down to show Console Content
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

    }



    public void CloseConsole()
    {
        consoleCanvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            consoleUIRoot.SetActive(false);
        });


        consoleTransitionScreen.alpha = 0;
        consoleTransitionScreen.gameObject.SetActive(false);
        HUDRoot.SetActive(true);
        DeltaBarRoot.SetActive(true);
        miningScreenUIRoot.SetActive(false);
        skillTreeScreenUIRoot.SetActive(false);
        prestigeScreenUIRoot.SetActive(false);
    }
}
