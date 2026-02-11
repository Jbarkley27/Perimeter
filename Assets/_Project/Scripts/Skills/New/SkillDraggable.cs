using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using NUnit.Framework;

[RequireComponent(typeof(CanvasGroup))]
public class SkillDraggable : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Animation")]
    public float snapDuration = 0.25f;
    public Ease snapEase = Ease.OutBack;
    public bool IsReturning { get; private set; }
    public bool Isre;


    [Header("UI References")]
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    public LoadoutDropTarget currentSlot;
    public Transform shadowRoot; // Optional shadow root for better visibility while dragging

    [Header("Skill Data")]
    public bool IsDragging { get; private set; } = false;
    public SkillData skillData;
    public SkillNodeUI nodeUI;




    void Awake()
    {
        CacheOriginalTransform();
    }


    void Update()
    {
        canvasGroup.blocksRaycasts = !IsReturning && !IsDragging;
    }




    #region Drag Handlers

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only non passive skills can be dragged
        // if (skillData == null || skillData.isPassive
        //     || nodeUI == null || SkillTreeRuntime.Instance == null
        //     || !SkillTreeRuntime.Instance.IsNodeActive(nodeUI.nodeDefinition))
        // {
        //     Debug.Log("Cannot drag passive or inactive skills.");
        //     return;
        // }

        // if (SkillTreeUIManager.Instance != null && SkillTreeUIManager.Instance.parentScrollRect != null)
        //     SkillTreeUIManager.Instance.parentScrollRect.enabled = false;

        // Hide hover info when dragging skill
        if(nodeUI != null)
        {
            nodeUI.ForcePointerExit();
        }



        // Clear slot if assigned. This allows the skill to be returned to original position
        // since and not be stuck in the slot.
        if (currentSlot)
        {
            currentSlot.ClearSlot();
            currentSlot = null;
        }


        // Disable parent scroll rect so that when dragging the skill it 
        // doesn't drag the background scroll rect.
        // if (SkillTreeUIManager.Instance.parentScrollRect != null)
        //     SkillTreeUIManager.Instance.parentScrollRect.enabled = false;


        // Adjust canvas group for dragging
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
        transform.SetParent(canvas.transform);
        rectTransform.DOKill();

        IsDragging = true;

        // Remove skill from loadout manager
        if (skillData) SkillLoadout.Instance.UnequipSkill(skillData);
    }


    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        // GlobalDataStore.Instance.SkillTreeUIController.HideInfo();
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        // if (SkillTreeUIManager.Instance != null && SkillTreeUIManager.Instance.parentScrollRect != null)
        //     SkillTreeUIManager.Instance.parentScrollRect.enabled = true;

        // Restore canvas group
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // If still parented to canvas invalid drop
        if (transform.parent == canvas.transform)
        {
            ReturnToOriginalPosition();
            Snapping = false;
        }
        else
        {
            Snapping = true;
        }

        IsDragging = false;

        // Re-enable parent scroll rect
        // if (SkillTreeUIManager.Instance.parentScrollRect != null)
        //     SkillTreeUIManager.Instance.parentScrollRect.enabled = true;
    }

    #endregion

    
    public bool Snapping = false;
    public void SnapToSlot(LoadoutDropTarget slot, bool notifyLoadout = true)
    {
        Snapping = true;

        // Leave old slot
        if (currentSlot != null)
            currentSlot.ClearSlot();

        // Assign new slot
        currentSlot = slot;
        slot.Assign(this);

        // Assign to slot parent
        transform.SetParent(slot.snapParent);

        // Animate to slot position
        rectTransform.DOKill();
        rectTransform.DOAnchorPos(Vector2.zero, snapDuration)
            .SetEase(snapEase)
            .OnKill(() => Snapping = false)
            .OnComplete(() =>
            {
                Snapping = false;
                slot.CollapseHover();
            });

        // equip skill in loadout manager
        if (notifyLoadout && skillData) SkillLoadout.Instance.EquipSkill(skillData);

        // Animate loadout icon to give feedback
        // if (SkillTreeUIManager.Instance != null && SkillTreeUIManager.Instance.skillLoadoutIcon != null)
        // {
        //     SkillTreeUIManager.Instance.skillLoadoutIcon.transform.DOPunchScale(Vector3.one * 1.2f, 0.4f, 1, 0.5f)
        //         .SetEase(Ease.OutCubic)
        //         .OnComplete(() =>
        //         {
        //             SkillTreeUIManager.Instance.skillLoadoutIcon.transform.localScale = Vector3.one;
        //             Snapping = false;
        //         });
        // }
    }


    

    private Tween returnTween;

    public void ReturnToOriginalPosition()
    {
        // Kill old tween if any, and clear state safely
        if (returnTween != null && returnTween.IsActive())
        {
            returnTween.Kill();
            returnTween = null;
            IsReturning = false; // ensure we don't get stuck
            Isre = false;
        }

        IsReturning = true;
        Isre = false;
        // canvasGroup.blocksRaycasts = false;

        transform.SetParent(originalParent);

        returnTween = rectTransform.DOAnchorPos(originalAnchoredPosition, snapDuration)
            .SetEase(snapEase)
            .SetUpdate(true) // optional: finishes even if timeScale=0
            .OnKill(() => { IsReturning = false; })
            .OnComplete(() =>
            {
                IsReturning = false;
                // canvasGroup.blocksRaycasts = true;
                Isre = false;
                returnTween = null;
            });
    }




    // Re-caches the origin after runtime layout/spawn.
    public void CacheOriginalTransform()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;
        originalAnchoredPosition = shadowRoot.GetComponent<RectTransform>().anchoredPosition;
    }

}
