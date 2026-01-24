using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalAnchoredPosition;

    private LoadoutDropTarget currentSlot;
    public bool IsSlotted => currentSlot != null;
    public bool IsDragging { get; private set; } = false;



    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store ORIGINAL position (before slot)
        if (currentSlot == null)
        {
            originalParent = transform.parent;
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }
        else
        {
            currentSlot.ClearSlot();
            currentSlot = null;
        }

        if (SkillTreeUIManager.Instance.parentScrollRect != null)
            SkillTreeUIManager.Instance.parentScrollRect.enabled = false;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;

        transform.SetParent(canvas.transform);
        rectTransform.DOKill();

        IsDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // If still parented to canvas → invalid drop
        if (transform.parent == canvas.transform)
        {
            ReturnToOriginalPosition();
        }

        IsDragging = false;

        if (SkillTreeUIManager.Instance.parentScrollRect != null)
            SkillTreeUIManager.Instance.parentScrollRect.enabled = true;
    }

    public void SnapToSlot(LoadoutDropTarget slot)
    {
        // Leave old slot
        if (currentSlot != null)
            currentSlot.ClearSlot();

        currentSlot = slot;
        slot.Assign(this);

        transform.SetParent(slot.snapParent);

        rectTransform.DOKill();
        rectTransform.DOAnchorPos(Vector2.zero, snapDuration)
            .SetEase(snapEase);
    }

    public void ReturnToOriginalPosition()
    {
        // Leaving slot
        if (currentSlot != null)
        {
            currentSlot.ClearSlot();
            currentSlot = null;
        }

        transform.SetParent(originalParent);

        rectTransform.DOKill();
        rectTransform.DOAnchorPos(originalAnchoredPosition, snapDuration)
            .SetEase(snapEase);
    }
}
