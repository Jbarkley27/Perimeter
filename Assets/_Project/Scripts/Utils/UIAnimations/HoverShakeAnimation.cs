using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HoverShakeAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float shakeDuration = 0.5f;
    public float shakeStrength = 10f;
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f;

    private Tween currentTween;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            transform.localScale = Vector3.one; // Reset scale before starting a new shake
        }

        currentTween = transform.DOPunchScale(transform.localScale * shakeStrength, shakeDuration, shakeVibrato, shakeRandomness)
            .SetEase(Ease.InOutElastic)
            .OnComplete(() =>
            {
                // Ensure the scale is reset to original after the shake
                transform.localScale = Vector3.one;
            });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            transform.localScale = Vector3.one; // Reset scale before starting a new shake
        }
    }
}