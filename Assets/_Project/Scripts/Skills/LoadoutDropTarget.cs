using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;


public class LoadoutDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    public Transform snapParent;
    public GameObject hoverVisual;

    [Header("Hover Animation")]
    public float hoverScale = 1.1f;
    public float hoverAnimDuration = 0.15f;
    public Ease hoverEase = Ease.OutBack;


    [Header("Runtime State")]
    private SkillDraggable occupiedItem;
    public Vector3 hoverOriginalScale = Vector3.one * .5f;
    public Vector3 hoverOccupiedScale = Vector3.one * 1.0f;
    public bool IsOccupied => occupiedItem != null;



    void Awake()
    {
        if (snapParent == null)
            snapParent = transform;

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
        if (IsOccupied)
            return;

        if (eventData.pointerDrag == null)
            return;

        if (!eventData.pointerDrag.CompareTag("Draggable"))
            return;

        var draggable = eventData.pointerDrag.GetComponent<SkillDraggable>();
        if (draggable == null)
            return;

        draggable.SnapToSlot(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
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
}
