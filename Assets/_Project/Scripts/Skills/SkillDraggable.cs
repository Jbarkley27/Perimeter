using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class SkillDraggable : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Animation")]
    public float snapDuration = 0.25f;
    public Ease snapEase = Ease.OutBack;

    [Header("UI References")]
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private LoadoutDropTarget currentSlot;

    [Header("Skill Data")]
    public bool IsDragging { get; private set; } = false;
    public SkillData skillData;
    public TreeNode treeNode;



    void Awake()
    {
        // Cache references
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }


    #region Drag Handlers

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only non passive skills can be dragged
        if (skillData == null || skillData.isPassive
            || treeNode == null || !SkillTreeData.Instance.IsNodeActive(treeNode))
            return;

        // Clear slot if assigned. This allows the skill to be returned to original position
        // since and not be stuck in the slot.
        if (currentSlot)
        {
            currentSlot.ClearSlot();
            currentSlot = null;
        }


        // Disable parent scroll rect so that when dragging the skill it 
        // doesn't drag the background scroll rect.
        if (SkillTreeUIManager.Instance.parentScrollRect != null)
            SkillTreeUIManager.Instance.parentScrollRect.enabled = false;


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
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore canvas group
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // If still parented to canvas invalid drop
        if (transform.parent == canvas.transform)
        {
            ReturnToOriginalPosition();
        }

        IsDragging = false;

        // Re-enable parent scroll rect
        if (SkillTreeUIManager.Instance.parentScrollRect != null)
            SkillTreeUIManager.Instance.parentScrollRect.enabled = true;
    }

    #endregion

    

    public void SnapToSlot(LoadoutDropTarget slot)
    {
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
            .OnComplete(() =>
            {
                slot.CollapseHover();
            });

        // equip skill in loadout manager
        if (skillData) SkillLoadout.Instance.EquipSkill(skillData);

        // Animate loadout icon to give feedback
        SkillTreeUIManager.Instance.skillLoadoutIcon.transform.DOPunchScale(Vector3.one * 1.2f, 0.4f, 1, 0.5f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                SkillTreeUIManager.Instance.skillLoadoutIcon.transform.localScale = Vector3.one;
            });
    }

    public void ReturnToOriginalPosition()
    {
        // Leaving slot, clear it
        if (currentSlot != null)
        {
            currentSlot.ClearSlot();
            currentSlot = null;
        }

        // Return to original parent and position
        transform.SetParent(originalParent);

        // Animate back to original position
        rectTransform.DOKill();
        rectTransform.DOAnchorPos(originalAnchoredPosition, snapDuration)
            .SetEase(snapEase);
    }
}
