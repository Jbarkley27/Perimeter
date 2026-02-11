using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ConsoleUIManager : MonoBehaviour
{
    // public enum ConsoleUIScreenState
    // {
    //     MINING = 0,
    //     SKILL_TREE = 1,
    //     PRESTIGE = 2
    // }

    [Header("General Console UI Elements")]
    // public ConsoleUIScreenState CurrentScreenState = ConsoleUIScreenState.SKILL_TREE;

    public static ConsoleUIManager Instance { get; private set; }

    // public Image consoleBackgroundImage;
    public GameObject consoleUIRoot;
    public CanvasGroup consoleCanvasGroup;
    public bool OpeningConsole = false;
    public GameObject HUDRoot;
    public GameObject DeltaBarRoot;
    // public CanvasGroup consoleTransitionScreen;
    // public float consoleOpenDelay = 0.1f;
    // public float transitionScreenOpenEndScale = 10f;


    // [Header("Mining Screen UI Elements")]
    // public GameObject miningScreenUIRoot;
    // public Image miningNavElementActiveIndicator;
    

    // [Header("Skill Tree Screen UI Elements")]
    // public GameObject skillTreeScreenUIRoot;
    // public Image skillTreeNavElementActiveIndicator;


    // [Header("Prestige Screen UI Elements")]
    // public GameObject prestigeScreenUIRoot;
    // public Image prestigeNavElementActiveIndicator;


    // [Header("Skill Tree Ring UI Elements")]
    // public Transform skillTreeRingRoot;


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
        yield return new WaitForSeconds(0);
    }



    public void CloseConsole()
    {
        consoleCanvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            consoleUIRoot.SetActive(false);
        });

        HUDRoot.SetActive(true);
        DeltaBarRoot.SetActive(true);
    }
}
