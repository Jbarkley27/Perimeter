using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFlasher : MonoBehaviour
{
    [Header("Flash Settings")]
    public float flashSpeed = 0.5f;
    [Range(0f, 1f)] public float alphaMin = 0.2f;
    [Range(0f, 1f)] public float alphaMax = 1f;

    private CanvasGroup canvasGroup;
    private Tween flashTween;
    public bool IsFlashing;
    public bool AutoStart = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (AutoStart) StartFlashing();
    }

    void OnEnable()
    {
        if (AutoStart) StartFlashing();
    }

    public void StartFlashing()
    {
        if (IsFlashing) return;
        StopFlashing(); // ensure no duplicates
        IsFlashing = true;
        flashTween = canvasGroup.DOFade(alphaMin, flashSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void StopFlashing(float overrideEndValue = -1)
    {
        if (!IsFlashing) return;
        if (flashTween != null && flashTween.IsActive())
        {
            flashTween.Kill();
            canvasGroup.alpha = overrideEndValue == -1 ? alphaMax : overrideEndValue; // reset alpha
            IsFlashing = false;
        }
    }

    void OnDisable()
    {
        StopFlashing();
    }
}
