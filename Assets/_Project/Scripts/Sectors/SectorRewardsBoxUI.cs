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
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverPanelRoot != null)
            hoverPanelRoot.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverPanelRoot != null)
            hoverPanelRoot.SetActive(false);
    }

    // Called by the button (or by auto-accept later).
    public void AcceptRewards(bool autoAccepted)
    {
        if (rewardsAccepted || boundRewards.Count == 0)
            return;

        rewardsAccepted = true;

        string rewardSummary = "Rewards:";
        for (int i = 0; i < boundRewards.Count; i++)
            rewardSummary += $" {boundRewards[i].rewardName}";

        Debug.Log(autoAccepted
            ? $"[Auto Accept] {rewardSummary}"
            : $"[Accept] {rewardSummary}");

        ShowRewardReceivedText(rewardSummary);
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
}
