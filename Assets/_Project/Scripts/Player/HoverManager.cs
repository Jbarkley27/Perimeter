using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HoverManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Sequence hoverSequence;

    void Start()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSequence != null && hoverSequence.IsActive())
        {
            hoverSequence.Kill();
        }

        hoverSequence = DOTween.Sequence();
        hoverSequence.Append(transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack));

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverSequence != null && hoverSequence.IsActive())
        {
            hoverSequence.Kill();
        }

        hoverSequence = DOTween.Sequence();
        hoverSequence.Append(transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
    }
}
