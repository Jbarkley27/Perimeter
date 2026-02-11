using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class PunchClickAnimation : MonoBehaviour, IPointerClickHandler
{
    public float punchScaleAmount = 1.2f;
    public float punchDuration = 0.2f;
    public int punchVibrato = 10;
    public float punchElasticity = 1f;

    private Tween currentTween;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Kill any existing tween to avoid overlapping animations
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            transform.localScale = Vector3.one; // Reset scale before starting a new punch
        }

        // Create a punch scale animation
        currentTween = transform.DOPunchScale(Vector3.one * (punchScaleAmount - 1), punchDuration, punchVibrato, punchElasticity)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Ensure the scale is reset to original after the punch
                transform.localScale = Vector3.one;
            });
    }
}