using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class SignalManager : MonoBehaviour
{
    [Header("Signal Values")]
    public double startingMaxSignalSeconds = 60;
    public double maxSignalSeconds;
    public double signalSeconds;
    public double decayPerSecond = 1.0;

    [Header("Damage Values")]
    public Slider damageSignalSlider;
    public TMP_Text damageSignalText;
    public int requiredDamageToWin = 100;
    public int totalDamageDealt = 0;

    [Header("UI")]
    public Slider signalSlider;
    public TMP_Text signalText;

    private bool isActive = true;
    public ParticleSystem broadcastSignalEffect;

    public static SignalManager Instance { get; private set; }

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
    public int totalGlassCollected = 0;
    public int totalEnemiesDefeated = 0;
    
    [Header("Sector")]
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
        UpdateUI();
    }

    private void Update()
    {
        if (!isActive) return;

        DecreaseSignal(Time.deltaTime * decayPerSecond);
    }

    // -----------------------------
    // PUBLIC API
    // -----------------------------

    public void ResetSignalUI()
    {
        maxSignalSeconds = startingMaxSignalSeconds;
        signalSeconds = maxSignalSeconds;


        // Reset end run stats
        totalGlassCollected = 0;
        totalEnemiesDefeated = 0;
        totalDamageDealt = 0;


        UpdateUI();
        PauseSignal();
        HideEndRunScreen();
    }

    public void PauseSignal()
    {
        isActive = false;
    }

    public void ResumeSignal()
    {
        isActive = true;
    }


    public void DecreaseSignal(double amount)
    {
        if (signalSeconds <= 0)
            return;

        signalSeconds -= amount;
        signalSeconds = System.Math.Max(signalSeconds, 0);
        UpdateUI();

        if (signalSeconds <= 0)
            OnSignalDepleted();
    }

    // -----------------------------
    // INTERNAL
    // -----------------------------

    private void OnSignalDepleted()
    {
        isActive = false;
        Debug.Log("Signal depleted — run ends");

        // End the run
        GameManager.Instance.EndRun();

        // Refill signal for next run
        signalSeconds = maxSignalSeconds;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (signalSlider)
            signalSlider.value = (float)(signalSeconds / maxSignalSeconds);

        if (signalText)
            signalText.text = FormatSignalTime(signalSeconds);

        if (damageSignalSlider)
            damageSignalSlider.value = (float)totalDamageDealt / requiredDamageToWin;

        if (damageSignalText)
            damageSignalText.text = damageSignalText.text = $"{totalDamageDealt} / {requiredDamageToWin}";
    }

    public void UpdateEndRunStatsUI()
    {
        if (totalDamageDealtText)
            totalDamageDealtText.DOText(totalDamageDealt.ToString(), 1);

        if (totalEnemiesDefeatedText)
            totalEnemiesDefeatedText.DOText(totalEnemiesDefeated.ToString(), 1);

        if (glassCollectedText)
            glassCollectedText.DOText(totalGlassCollected.ToString(), 1);

        if (endRunDamageSlider)
            endRunDamageSlider.DOValue((float)totalDamageDealt / requiredDamageToWin, 1).SetEase(Ease.OutCubic);

        if (currentHeaderSectorText)
            currentHeaderSectorText.text = $"Sector {SectorManager.Instance.currentSectorIndex + 1}";

        if (nextSectorText)
            nextSectorText.text = SectorManager.Instance.GetNextSectorIndex()  + "";

        if (currentSectorExtraText)
            currentSectorExtraText.text = SectorManager.Instance.GetCurrentSectorIndex() + "";
    }

    private string FormatSignalTime(double seconds)
    {
        if (seconds < 60)
            return $"{seconds:F1}s";

        if (seconds < 3600)
        {
            int mins = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{mins}m {secs}s";
        }

        if (seconds < 86400)
        {
            double hours = seconds / 3600;
            return $"{hours:F1}h";
        }

        if (seconds < 86400 * 100)
        {
            double days = seconds / 86400;
            return $"{days:F1}d";
        }

        return $"{seconds:E1}s";
    }

    public IEnumerator BroadcastSignal()
    {
        if (broadcastSignalEffect != null)
        {
            broadcastSignalEffect.Play();
            // broadcastSignalEffect2.Play();
        }

        // Assume the particle effect lasts 3 seconds
        yield return new WaitForSeconds(3f);

        if (broadcastSignalEffect != null)
        {
            broadcastSignalEffect.Stop();
            // broadcastSignalEffect2.Stop();
        }

        // ResumeSignal();
    }


    public void AddDamage(int damageAmount)
    {
        totalDamageDealt += damageAmount;
        totalDamageDealt = Mathf.Min(totalDamageDealt, requiredDamageToWin);

        if (damageSignalText)
            damageSignalText.text = $"{totalDamageDealt} / {requiredDamageToWin}";

        if (damageSignalSlider)
            damageSignalSlider.value = (float)totalDamageDealt / requiredDamageToWin;

        if (totalDamageDealt >= requiredDamageToWin)
        {
            // Player has dealt enough damage to win
            Debug.Log("Required damage dealt! You win!");
            GameManager.Instance.EndRun();
        }
    }

    public void CollectGlass(EnemyPooler.EnemyType enemyType)
    {
        totalGlassCollected += EnemyDataStore.Instance.GetGlassRewardForEnemyType(enemyType);
    }

    public void DefeatEnemy()
    {
        totalEnemiesDefeated += 1;
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
                child.DOPunchScale(child.localScale * .1f, 0.15f, 10, 1);

            // only show last child after a delay
            if (child == rootViewParent.GetChild(rootViewParent.childCount - 1))
            {
                // only display if the user has dealt enough damage to win
                if (totalDamageDealt >= requiredDamageToWin)
                {
                    yield return new WaitForSeconds(0.1f);
                    // enable this child
                    cg.DOFade(1, 0.15f).SetDelay(0.5f);
                    child.DOPunchScale(child.localScale * .1f, 0.15f, 10, 1);
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