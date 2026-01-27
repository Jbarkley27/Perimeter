using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("End Run Screen")]
    public GameObject endRunScreen;
    public Image endRunBorderImage;
    public Transform rootViewParent;
    public bool endRunScreenChanging = false;
    public Coroutine endRunScreenCoroutine;
    public CanvasGroup endRunCanvasGroup;
    public GameObject restartRunButton;

    [Header("End Run Stats")]
    public TMP_Text totalDamageDealtText;
    public TMP_Text totalEnemiesDefeatedText;
    public TMP_Text glassCollectedText;
    public Slider endRunDamageSlider;
    public TMP_Text currentHeaderSectorText;
    public TMP_Text nextSectorText;
    public TMP_Text currentSectorExtraText;



    public void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    // -----------------------------
    // PUBLIC API
    // -----------------------------

    public void ResetRun()
    {
        HideEndRunScreen();
    }


    public void UpdateEndRunStatsUI()
    {
        if (totalDamageDealtText)
            totalDamageDealtText.DOText(EnemyManager.Instance.GetTotalDamageDealtToEnemiesThisRun().ToString(), 1);
            

        if (totalEnemiesDefeatedText)
            totalEnemiesDefeatedText.DOText(EnemyManager.Instance.GetTotalEnemiesDefeatedThisRun().ToString(), 1);

        if (glassCollectedText)
            glassCollectedText.DOText(GlassManager.Instance.GetCurrentGlassShards().ToString(), 1);

        // End Run Damage Slider Animation to next slider
        if (endRunDamageSlider)
            endRunDamageSlider.maxValue = (float)EnemyManager.Instance.requiredDamageToWin;
            endRunDamageSlider.DOValue((float)(EnemyManager.Instance.GetTotalDamageDealtToEnemiesThisRun() / EnemyManager.Instance.requiredDamageToWin), 1).SetEase(Ease.OutCubic);

        if (currentHeaderSectorText)
            currentHeaderSectorText.text = $"Sector {SectorManager.Instance.currentSectorIndex + 1}";

        if (nextSectorText)
            nextSectorText.text = SectorManager.Instance.GetNextSectorIndex()  + "";

        if (currentSectorExtraText)
            currentSectorExtraText.text = SectorManager.Instance.GetCurrentSectorIndex() + "";
    }




    public void StartShowEndRunScreen()
    {
        if (endRunScreenCoroutine != null)
            StopCoroutine(endRunScreenCoroutine);

        endRunScreenCoroutine = StartCoroutine(ShowEndRunScreen());
    }

    public IEnumerator ShowEndRunScreen()
    {
        if (endRunScreenChanging)
            yield break;

        yield return new WaitForSeconds(0.2f);

        endRunScreenChanging = true;
        // Turn off all canvas groups first
        foreach (Transform child in rootViewParent)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 0;
        }

        UpdateEndRunStatsUI();

        yield return new WaitForSeconds(0.2f);

        Color c = endRunBorderImage.color;
        c.a = Mathf.Clamp01(0);
        endRunBorderImage.color = c;

        endRunCanvasGroup.alpha = 1;

        // Activate end run screen
        endRunScreen.SetActive(true);



        endRunBorderImage.gameObject.SetActive(true);

        foreach (Transform child in rootViewParent)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.DOFade(1, 0.15f);
                
                

            // only show last child after a delay
            if (child == rootViewParent.GetChild(rootViewParent.childCount - 1))
            {
                // only display if the user has dealt enough damage to WIN
                if (EnemyManager.Instance.GetPercentOfEnemiesDefeatedInCurrentWave() >= 1f)
                {
                    yield return new WaitForSeconds(0.1f);

                    // enable this child
                    child.gameObject.SetActive(true);
                    cg.alpha = 0;
                    cg.DOFade(1, 0.15f).SetDelay(0.5f).SetEase(Ease.OutCubic);
                    Vector3 baseScale = child.localScale;

                    child.DOKill(); // VERY important
                    child.localScale = baseScale;

                    child.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1).SetDelay(0.5f)
                    .SetEase(Ease.OutCubic);

                    // instead the reward, go to next sector button will show
                    // which is already a apart of the last child
                    restartRunButton.SetActive(false);
                }
                else
                {
                    // Turn off Rewards
                    child.gameObject.SetActive(false);
                    restartRunButton.SetActive(true);
                }
            }
            yield return new WaitForSeconds(0.08f);
        }


        Color b = endRunBorderImage.color;
        b.a = Mathf.Clamp01(.6f);
        endRunBorderImage.color = b;

        endRunScreenChanging = false;
    }


    public void HideEndRunScreen()
    {
        if (endRunScreenChanging || endRunScreenCoroutine != null)
        {
            // Force stop any ongoing transition
            StopCoroutine(endRunScreenCoroutine);
            endRunScreenChanging = false;
        }



            
        endRunCanvasGroup.DOFade(0, 0.35f)
            .OnComplete(() =>
            {
                endRunScreenChanging = false;
                endRunScreen.SetActive(false);

                foreach (Transform child in rootViewParent)
                {
                    CanvasGroup cg = child.GetComponent<CanvasGroup>();
                    if (cg != null)
                        cg.alpha = 0;
                }
            });

    }
}


// TODO: Once we work on the Skill Tree system, we can add ways to modify the startingMaxSignalSeconds and decayPerSecond via upgrades.