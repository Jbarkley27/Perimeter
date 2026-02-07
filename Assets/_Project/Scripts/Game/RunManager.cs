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
    // public Slider endRunDamageSlider;
    public TMP_Text currentHeaderSectorText;
    // public TMP_Text nextSectorText;
    // public TMP_Text currentSectorExtraText;
    public TMP_Text victoryText;

    [Header("Compass UI")]
    public GameObject compassRoot;
    public SectorCompassUIController compassUI;
    public SectorRewardsBoxUI rewardsBox;

    [Header("Compass Visuals")]
    public Transform playerShipRoot;
    public Transform starfieldRoot;
    public float shipRotateDuration = 1f;
    public Ease shipRotateEase = Ease.OutCubic;
    public float starfieldSpeed = 8f;

    [Header("Background Transition")]
    public Camera backgroundCamera;
    public Color fallbackBackgroundColor = Color.black;
    public float backgroundLerpDuration = 1.5f;

    private readonly List<SectorRewardEntry> pendingEndRunRewards = new List<SectorRewardEntry>();
    private Color currentBackgroundColor;

    [Header("Active Modifier UI")]
    public SectorActiveModifierUI activeModifierUI;

    private SectorDirection? lastChosenDirection = null;





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

        if (endRunCanvasGroup == null && endRunScreen != null)
            endRunCanvasGroup = endRunScreen.GetComponent<CanvasGroup>();

    }

    private void Start()
    {
        if (backgroundCamera != null)
            currentBackgroundColor = backgroundCamera.backgroundColor;
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
            glassCollectedText.DOText(GlassManager.Instance.GetCurrentGlassShardsFormatted(), 1);

        // End Run Damage Slider Animation to next slider
        // if (endRunDamageSlider)
        //     endRunDamageSlider.maxValue = (float)EnemyManager.Instance.requiredDamageToWin;
        //     endRunDamageSlider.DOValue((float)(EnemyManager.Instance.GetTotalDamageDealtToEnemiesThisRun() / EnemyManager.Instance.requiredDamageToWin), 1).SetEase(Ease.OutCubic);

        if (currentHeaderSectorText)
            currentHeaderSectorText.text = $"Sector {SectorManager.Instance.currentSectorIndex + 1}";

        // if (nextSectorText)
        //     nextSectorText.text = SectorManager.Instance.GetNextSectorIndex()  + "";

        // if (currentSectorExtraText)
        //     currentSectorExtraText.text = SectorManager.Instance.GetCurrentSectorIndex() + "";

        if (victoryText)
            victoryText.text = WasRunVictory() 
                ? "Victory! Accept your rewards and set next course." 
                : "You were overwhelmed by The Swarm.";
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

        UpdateEndRunStatsUI();

        Color c = endRunBorderImage.color;

        endRunScreen.SetActive(true);
        if (endRunCanvasGroup != null)
        {
            endRunCanvasGroup.alpha = 1f;
            endRunCanvasGroup.interactable = true;
            endRunCanvasGroup.blocksRaycasts = true;
        }




        endRunScreenChanging = true;
        // Turn off all canvas groups first
        foreach (Transform child in rootViewParent)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 0;
        }

        UpdateEndRunStatsUI();

        bool isWin = WasRunVictory();

        if (isWin)
        {
            Debug.Log("Player won the run! Showing rewards and compass.");
            BuildEndRunRewards();
            if (activeModifierUI != null)
                activeModifierUI.Refresh();
            ShowCompassForWin();
        }
        else
        {
            Debug.Log("Player lost the run. Hiding rewards and compass.");
            if (SectorManager.Instance != null)
                SectorManager.Instance.ClearRunModifierState();

            HideCompass();
        }

        endRunCanvasGroup.alpha = 1;

        if (restartRunButton != null)
            restartRunButton.SetActive(!isWin);

        if (rewardsBox != null)
            rewardsBox.gameObject.SetActive(isWin);


        yield return new WaitForSeconds(0.2f);

        c = endRunBorderImage.color;
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

        HideCompass();
        if (activeModifierUI != null)
            activeModifierUI.Refresh();

    }


    // True if this run ended with a win.
    private bool WasRunVictory()
    {
        return EnemyManager.Instance != null
            && EnemyManager.Instance.HasDefeatedAllEnemiesInCurrentWave();
    }

    // Builds the rewards for the end-run box (current sector + active modifier).
    private void BuildEndRunRewards()
    {
        pendingEndRunRewards.Clear();

        Sector current = SectorManager.Instance != null ? SectorManager.Instance.GetCurrentSector() : null;
        int baseRewardCount = current != null && current.baseRewards != null ? current.baseRewards.Count : 0;
        if (current != null && current.baseRewards != null)
            pendingEndRunRewards.AddRange(current.baseRewards);

        SectorModifierDefinition active = SectorManager.Instance != null ? SectorManager.Instance.ActiveModifier : null;
        int modifierRewardCount = active != null && active.rewards != null ? active.rewards.Count : 0;

        Debug.Log($"[RunManager] BuildEndRunRewards sector={SectorManager.Instance?.GetCurrentSectorIndex()} baseRewards={baseRewardCount} activeModifier={(active != null ? active.displayName : "None")} modifierRewards={modifierRewardCount}");

        if (active != null && active.rewards != null)
            pendingEndRunRewards.AddRange(active.rewards);

        if (rewardsBox != null)
            rewardsBox.BindRewards(pendingEndRunRewards);

        Debug.Log($"[RunManager] Total rewards bound={pendingEndRunRewards.Count}");
    }

    // Auto-accept rewards if the player didn't click the box.
    public void TryAutoAcceptRewards(bool autoAccepted)
    {
        if (rewardsBox != null && !rewardsBox.RewardsAccepted)
            rewardsBox.AcceptRewards(autoAccepted);
    }

    // Shows compass inside end-run screen (win only).
    private void ShowCompassForWin()
    {
        if (SectorManager.Instance != null)
        {
            int nextSectorNumber = SectorManager.Instance.GetNextSectorIndex();
            SectorManager.Instance.EnsurePendingCompassChoices(nextSectorNumber);
        }

        if (compassRoot != null)
            compassRoot.SetActive(true);

        if (compassUI != null)
            compassUI.RefreshFromPending();
    }

    // Shows compass without end-run panel (returning from console).
    public bool TryShowCompassOnly()
    {
        if (SectorManager.Instance == null || !SectorManager.Instance.HasPendingCompassChoices())
            return false;

        if (compassRoot != null)
            compassRoot.SetActive(true);

        if (compassUI != null)
            compassUI.RefreshFromPending();

        return true;
    }

    public void HideCompass()
    {
        if (compassRoot != null)
            compassRoot.SetActive(false);

        if (compassUI != null)
            compassUI.HideInfo();

    }

    // Called by compass UI when a direction is selected.
    public void OnCompassChoiceSelected(SectorCompassChoice choice)
    {
        TryAutoAcceptRewards(true);

        // Apply animation to direction button


        if (SectorManager.Instance != null)
        {
            SectorManager.Instance.SelectCompassChoice(choice);
            if (activeModifierUI != null)
                activeModifierUI.Refresh();
            SectorManager.Instance.AdvanceToNextSector();
        }

        ApplySectorDirectionVisuals(choice.direction);
        ApplyBackgroundForCurrentSector();

        HideCompass();
        HideEndRunScreen();

        if (GameManager.Instance != null)
            GameManager.Instance.StartBattlePhase(false, true);

        if (compassUI != null)
            compassUI.HideInfo();

    }

    // Rotates the ship and updates starfield direction.
    private void ApplySectorDirectionVisuals(SectorDirection direction)
    {
        // Exit early if the same direction is chosen again (can happen when returning from console), but still update starfield in case it got reset.
        if (lastChosenDirection.HasValue && lastChosenDirection.Value == direction)
        {
            // Still update starfield (in case it got reset elsewhere).
            float yawSame = GetYawForDirection(direction);
            Vector3 forwardSame = Quaternion.Euler(0f, yawSame, 0f) * Vector3.forward;
            ApplyStarfieldVelocity(-forwardSame);
            return;
        }

        lastChosenDirection = direction;

        float yaw = GetYawForDirection(direction);

        if (playerShipRoot != null)
        {
            float currentYaw = GetCurrentYaw();
            float delta = Mathf.DeltaAngle(currentYaw, yaw);

            // Already facing target direction → no rotation
            if (Mathf.Abs(delta) < 0.1f)
            {
                Vector3 forwardD = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                ApplyStarfieldVelocity(-forwardD);
                return;
            }

            playerShipRoot.DOKill();
            playerShipRoot.DORotate(new Vector3(0f, yaw, 0f), shipRotateDuration, RotateMode.Fast)
                .SetEase(shipRotateEase);
        }

        Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        ApplyStarfieldVelocity(-forward);
    }

    // Updates all particle systems under starfield root.
    private void ApplyStarfieldVelocity(Vector3 direction)
    {
        if (starfieldRoot == null)
            return;

        Vector3 velocity = direction.normalized * starfieldSpeed;

        ParticleSystem[] systems = starfieldRoot.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in systems)
        {
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = velocity.x;
            vel.y = velocity.y;
            vel.z = velocity.z;
        }
    }

    // Lerp background color to current sector's color.
    private void ApplyBackgroundForCurrentSector()
    {
        if (backgroundCamera == null)
            return;

        Sector current = SectorManager.Instance != null ? SectorManager.Instance.GetCurrentSector() : null;
        Color target = current != null ? current.backgroundColor : fallbackBackgroundColor;

        DOTween.To(() => currentBackgroundColor,
            c =>
            {
                currentBackgroundColor = c;
                backgroundCamera.backgroundColor = c;
            },
            target,
            backgroundLerpDuration);
    }



    private float GetYawForDirection(SectorDirection direction)
    {
        switch (direction)
        {
            case SectorDirection.North: return 0f;
            case SectorDirection.East: return 90f;
            case SectorDirection.South: return 180f;
            case SectorDirection.West: return 270f;
            default: return 0f;
        }
    }

    private float GetCurrentYaw()
    {
        if (playerShipRoot == null)
            return 0f;

        return Mathf.Repeat(playerShipRoot.eulerAngles.y, 360f);
    }

}
