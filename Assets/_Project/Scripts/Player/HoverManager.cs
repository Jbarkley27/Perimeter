using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HoverManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Sequence hoverSequence;

    void Start()
    {
        // rangeUI.SetActive(false);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // rangeUI.SetActive(true);
        if (hoverSequence != null && hoverSequence.IsActive())
        {
            hoverSequence.Kill();
        }

        hoverSequence = DOTween.Sequence();
        hoverSequence.Append(transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack));

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // rangeUI.SetActive(false);
        if (hoverSequence != null && hoverSequence.IsActive())
        {
            hoverSequence.Kill();
        }

        hoverSequence = DOTween.Sequence();
        hoverSequence.Append(transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
    }
}
