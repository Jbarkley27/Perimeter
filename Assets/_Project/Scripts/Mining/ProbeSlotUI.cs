using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * ProbeSlotUI
 * -----------
 * Interactive probe slot UI.
 * Left-click upgrades, right-click refunds/despawns.
 * Hover shows the shared probe info panel.
 */
public class ProbeSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject occupiedVisual;
    public GameObject emptyVisual;
    public Image iconImage;
    public Slider levelSlider;
    public CanvasGroup canvasGroup;

    private Probe boundProbe;
    private Planet boundPlanet;
    private ProbeUIController owner;

    private Probe lastProbe;
    private int lastLevel = -1;
    private bool canUpgrade = false;


    // Bind this slot to a specific probe.
    public void Bind(Probe probe, Planet planet, ProbeUIController ownerController)
    {
        boundProbe = probe;
        boundPlanet = planet;
        owner = ownerController;

        bool probeChanged = boundProbe != lastProbe;
        lastProbe = boundProbe;

        if (probeChanged && boundProbe != null)
            lastLevel = boundProbe.Level;

        if (occupiedVisual) occupiedVisual.SetActive(true);
        if (emptyVisual) emptyVisual.SetActive(false);

        Refresh();
    }

    // Show as empty slot.
    public void ShowEmpty()
    {
        boundProbe = null;
        boundPlanet = null;
        owner = null;
        lastProbe = null;
        lastLevel = -1;

        if (occupiedVisual) occupiedVisual.SetActive(false);
        if (emptyVisual) emptyVisual.SetActive(true);

        if (iconImage)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }

        if (levelSlider)
            levelSlider.gameObject.SetActive(false);

        if (canvasGroup)
            canvasGroup.alpha = 1f;
    }

    // Refresh icon + slider visuals.
    public void Refresh()
    {
        if (boundProbe == null || boundPlanet == null)
            return;

        if (iconImage != null)
        {
            iconImage.sprite = ProbeManager.Instance != null
                ? ProbeManager.Instance.GetProbeIcon(boundProbe.Type)
                : null;

            iconImage.enabled = iconImage.sprite != null;
        }

        int newLevel = boundProbe.Level;
        bool levelIncreased = lastLevel >= 0 && newLevel > lastLevel;

        if (levelSlider != null)
        {
            levelSlider.gameObject.SetActive(true);
            levelSlider.minValue = 0;
            levelSlider.maxValue = boundPlanet.GetMaxProbeLevel();
            levelSlider.value = newLevel;
        }

        bool isMax = boundProbe.Level >= boundPlanet.GetMaxProbeLevel();
        bool canAfford = ProbeManager.Instance != null && ProbeManager.Instance.CanAffordUpgrade(boundProbe, boundPlanet);
        canUpgrade = !isMax && canAfford;

        if (canvasGroup != null)
            canvasGroup.alpha = canUpgrade ? 1f : 0.4f;


        if (levelIncreased && levelSlider != null)
            PunchTransform(levelSlider.transform, 0.08f, 0.15f);

        lastLevel = newLevel;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null && boundProbe != null && boundPlanet != null)
        {
            owner.ShowInfo(boundProbe, boundPlanet);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (owner != null)
            owner.HideInfo();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (boundProbe == null || boundPlanet == null || owner == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!canUpgrade)
                return;

            if (owner.TryUpgrade(boundProbe, boundPlanet))
                PunchTransform(transform, 0.12f, 0.2f);
        }

        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (owner.TryRefund(boundProbe, boundPlanet))
                PunchTransform(transform, 0.12f, 0.2f);
        }
    }

    // Punches a target transform and restores its scale on completion.
    private void PunchTransform(Transform target, float strength, float duration)
    {
        if (target == null)
            return;

        Vector3 original = target.localScale;
        target.DOKill();
        target.DOPunchScale(Vector3.one * strength, duration, 10, 1f)
            .OnComplete(() => target.localScale = original);
    }
}
