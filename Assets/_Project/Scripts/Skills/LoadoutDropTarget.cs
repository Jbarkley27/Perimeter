using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;


public class LoadoutDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    public Transform snapParent;
    public GameObject hoverVisual;
    public Transform scaleRoot;

    [Header("Hover Animation")]
    public float hoverScale = 1.1f;
    public float hoverAnimDuration = 0.15f;
    public Ease hoverEase = Ease.OutBack;
    public CanvasGroup hoverCanvasGroup;


    [Header("Runtime State")]
    private SkillDraggable occupiedItem;
    public Vector3 hoverOriginalScale = Vector3.one * .5f;
    public Vector3 hoverOccupiedScale = Vector3.one * 1.0f;
    public bool IsOccupied => occupiedItem != null;
    public SkillDraggable OccupiedItem => occupiedItem;

    public int SlotIndex { get; private set; } = -1;
    public bool IsActiveSlot { get; private set; }
    public bool IsDisabledSlot { get; private set; }
    public bool IsEnabledSlot { get; private set; }
    private Vector3 baseScale = Vector3.one;



    void Awake()
    {
        if (snapParent == null)
            snapParent = transform;

        if (scaleRoot == null)
            scaleRoot = transform;

        if (scaleRoot != null)
            baseScale = scaleRoot.localScale;

        if (hoverVisual != null)
            hoverOriginalScale = hoverVisual.transform.localScale;
    }



    public void Assign(SkillDraggable draggable)
    {
        occupiedItem = draggable;
        CollapseHover();
    }



    public void ClearSlot()
    {
        occupiedItem = null;
        CollapseHover();
    }



    public void OnDrop(PointerEventData eventData)
    {
        SkillDraggable draggable = null;
        if (eventData.pointerDrag != null)
            draggable = eventData.pointerDrag.GetComponent<SkillDraggable>();

        if (SlotIndex >= 0 && (!IsActiveSlot || IsDisabledSlot))
        {
            Debug.Log("Drop blocked: slot is not active.");
            if (draggable != null)
            {
                draggable.Snapping = false;
                draggable.ReturnToOriginalPosition();
            }
            return;
        }

        if (IsOccupied)
        {
            Debug.Log("Slot is already occupied!");
            if (draggable != null)
            {
                draggable.Snapping = false;
                draggable.ReturnToOriginalPosition();
            }
            return;
        }

        if (eventData.pointerDrag == null)
        {
            Debug.Log("No draggable item detected in drop event.");
            return;
        }

        if (!eventData.pointerDrag.CompareTag("Draggable"))
        {
            Debug.Log("Dropped item does not have the 'Draggable' tag.");
            return;
        }

        if (draggable == null)
        {
            Debug.Log("Dropped item does not have a SkillDraggable component.");
            return;
        }

        draggable.Snapping = true;
        draggable.SnapToSlot(this, true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SlotIndex >= 0 && (!IsActiveSlot || IsDisabledSlot))
            return;

        if (IsOccupied)
            return;

        if (hoverVisual == null)
            return;

        hoverVisual.transform.DOKill();
        hoverVisual.transform.DOScale(
            hoverOriginalScale * hoverScale,
            hoverAnimDuration
        ).SetEase(hoverEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverVisual == null)
            return;

        CollapseHover();
    }

    

    public void CollapseHover()
    {
        if (hoverVisual == null)
            return;

        hoverVisual.transform.DOKill();
        hoverVisual.transform.DOScale(
            IsOccupied ? hoverOccupiedScale : hoverOriginalScale,
            hoverAnimDuration
        ).SetEase(hoverEase);
    }

    public void SetSlotState(int index, bool isActive, bool isDisabled, bool isEnabled, float scaleMult)
    {
        SlotIndex = index;
        IsActiveSlot = isActive;
        IsDisabledSlot = isDisabled;
        IsEnabledSlot = isEnabled;

        gameObject.SetActive(isEnabled);

        if (scaleRoot != null)
            scaleRoot.localScale = baseScale * scaleMult;
    }
}
