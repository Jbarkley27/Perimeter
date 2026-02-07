using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/*
 * SectorRewardsBoxUI
 * ------------------
 * Reward box button on the end-run panel.
 * Hover shows a stacked rewards panel. Click accepts all rewards.
 */
public class SectorRewardsBoxUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button")]
    public Button rewardButton;

    [Header("Hover Panel")]
    public GameObject hoverPanelRoot;
    public SectorRewardListUI rewardList;

    [Header("Accepted Text")]
    public TMP_Text rewardReceivedText;
    public CanvasGroup rewardReceivedCanvasGroup;
    public float rewardMessageFadeTime = 0.25f;
    public float rewardMessageHoldTime = 1.5f;

    private List<SectorRewardEntry> boundRewards = new List<SectorRewardEntry>();

    private bool rewardsAccepted = false;
    public bool RewardsAccepted => rewardsAccepted;
    [Header("Follow Mouse")]
    public Vector3 mouseOffset = new Vector3(15f, -15f, 0f);
    public Canvas rootCanvas;
    private bool isHovering;
    private RectTransform hoverPanelRect;



    private void Awake()
    {
        if (rewardButton == null)
            rewardButton = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);

        if (rewardButton != null)
        {
            rewardButton.onClick.RemoveAllListeners();
            rewardButton.onClick.AddListener(() => AcceptRewards(false));
        }

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (hoverPanelRoot != null)
            hoverPanelRect = hoverPanelRoot.GetComponent<RectTransform>();

        SetHoverPanelVisible(false);
    }

    private void Update()
    {
        if (!isHovering || hoverPanelRoot == null || !hoverPanelRoot.activeSelf)
            return;

        FollowMousePosition(GetCursorPosition());
    }


    private void OnEnable()
    {
        SetHoverPanelVisible(false);
    }

    private void OnDisable()
    {
        SetHoverPanelVisible(false);
    }

    private void SetHoverPanelVisible(bool visible)
    {
        if (hoverPanelRoot == null)
            return;

        hoverPanelRoot.SetActive(visible);

        // Make sure the hover panel doesn't block raycasts.
        CanvasGroup cg = hoverPanelRoot.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }



    // Binds the rewards that will be shown and accepted.
    public void BindRewards(List<SectorRewardEntry> rewards)
    {
        boundRewards.Clear();
        if (rewards != null)
            boundRewards.AddRange(rewards);

        rewardsAccepted = false;

        if (rewardReceivedCanvasGroup != null)
            rewardReceivedCanvasGroup.gameObject.SetActive(false);

        if (rewardList != null)
            rewardList.SetRewards(boundRewards);

        Debug.Log($"[SectorRewardsBoxUI] BindRewards count={boundRewards.Count} rewardsNull={(rewards == null)}");
        if (boundRewards.Count > 0)
        {
            for (int i = 0; i < boundRewards.Count; i++)
                Debug.Log($"[SectorRewardsBoxUI] Reward[{i}] {boundRewards[i].rewardName} ({boundRewards[i].rewardType})");
        }

        SetHoverPanelVisible(false);

    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        SetHoverPanelVisible(true);
        FollowMousePosition(GetCursorPosition());

        gameObject.transform.DOPunchScale(Vector3.one * .1f, 0.25f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // reset scale to ensure no drift
                gameObject.transform.localScale = Vector3.one;
            });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        SetHoverPanelVisible(false);
    }



    // Called by the button (or by auto-accept later).
    public void AcceptRewards(bool autoAccepted)
    {
        if (rewardsAccepted || boundRewards.Count == 0)
            return;

        Debug.Log($"[SectorRewardsBoxUI] AcceptRewards called auto={autoAccepted} accepted={rewardsAccepted} boundCount={boundRewards.Count} glassTotal={(GlassManager.Instance != null ? GlassManager.Instance.GetTotalGlassShardsCollected() : -1)}");


        rewardsAccepted = true;

        string rewardSummary = "Rewards:";
        for (int i = 0; i < boundRewards.Count; i++)
            rewardSummary += $" {boundRewards[i].rewardName}";

        Debug.Log(autoAccepted
            ? $"[Auto Accept] {rewardSummary}"
            : $"[Accept] {rewardSummary}");

        ShowRewardReceivedText(rewardSummary);
        ApplyRewards();
        
        SetHoverPanelVisible(false);
        hoverPanelRoot?.SetActive(false);
        gameObject.SetActive(false);

    }


    private void ShowRewardReceivedText(string message)
    {
        if (rewardReceivedText != null)
            rewardReceivedText.text = message;

        if (rewardReceivedCanvasGroup == null)
            return;

        rewardReceivedCanvasGroup.DOKill();
        rewardReceivedCanvasGroup.alpha = 0f;
        rewardReceivedCanvasGroup.gameObject.SetActive(true);

        rewardReceivedCanvasGroup.DOFade(1f, rewardMessageFadeTime)
            .OnComplete(() =>
            {
                rewardReceivedCanvasGroup.DOFade(0f, rewardMessageFadeTime)
                    .SetDelay(rewardMessageHoldTime);
            });
    }


    // Executes reward effects (currently Glass-only).
    private void ApplyRewards()
    {
        if (boundRewards == null || boundRewards.Count == 0)
            return;


        foreach (var reward in boundRewards)
        {
            double before = GlassManager.Instance != null
                ? GlassManager.Instance.GetTotalGlassShardsCollected()
                : -1;

            Debug.Log($"[SectorRewardsBoxUI] Applying {reward.rewardType} value={reward.rewardValue} before={before}");

            switch (reward.rewardType)
            {
                case SectorRewardType.GlassFlat:
                    if (GlassManager.Instance != null)
                        GlassManager.Instance.AddGlass(reward.rewardValue);
                    break;

                case SectorRewardType.GlassPercentCurrent:
                    if (GlassManager.Instance != null)
                    {
                        double current = GlassManager.Instance.GetCurrentGlassShards();
                        GlassManager.Instance.AddGlass(current * (reward.rewardValue / 100f));
                    }
                    break;

                case SectorRewardType.GlassPercentTotal:
                    if (GlassManager.Instance != null)
                    {
                        double total = GlassManager.Instance.GetTotalGlassShardsCollected();
                        GlassManager.Instance.AddGlass(total * (reward.rewardValue / 100f));
                    }
                    break;

                case SectorRewardType.AugmentChance:
                    Debug.Log($"[Reward] Augment chance bonus: {reward.rewardValue}%");
                    break;

                case SectorRewardType.Placeholder:
                default:
                    Debug.Log($"[Reward] {reward.rewardName}");
                    break;
            }

            double after = GlassManager.Instance != null
                ? GlassManager.Instance.GetTotalGlassShardsCollected()
                : -1;

            Debug.Log($"[SectorRewardsBoxUI] Applied {reward.rewardType} after={after}");
        }

    }

    private Vector3 GetCursorPosition()
    {
        if (WorldCursor.instance != null)
            return WorldCursor.instance.GetCursorPosition();

        return Input.mousePosition;
    }

    private void FollowMousePosition(Vector3 mousePosition)
    {
        if (hoverPanelRect == null)
            return;

        RectTransform parentRect = hoverPanelRect.parent as RectTransform;
        if (parentRect == null)
            return;

        Camera cam = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            mousePosition,
            cam,
            out Vector2 localPoint
        );

        hoverPanelRect.anchoredPosition = localPoint + (Vector2)mouseOffset;
    }
}
