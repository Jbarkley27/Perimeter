using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// Handles purchase button behavior for a specific probe type.
public class ProbeBuyButton : MonoBehaviour
{
    public ProbeType type;
    public Image iconImage;
    public Button button;
    public CanvasGroup canvasGroup;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        MiningManager.Instance.miningUI.RegisterBuyButton(this);
    }

    // Refreshes the icon on enable (safe if ProbeManager is ready).
    private void OnEnable()
    {
        RefreshIcon();
    }

    void Update()
    {
        bool unlocked = ProbeManager.Instance.IsProbeUnlocked(type);
        button.gameObject.SetActive(unlocked);

        button.interactable = CanPurchase();
        canvasGroup.alpha = button.interactable ? 1f : 0.4f;
    }

    public bool CanPurchase()
    {
        return ProbeManager.Instance.CanAffordProbe(type)
            && ProbeManager.Instance.IsProbeUnlocked(type)
            && MiningManager.Instance.CurrentPlanet != null
            && !MiningManager.Instance.IsCurrentPlanetProbesFull();
    }

    // Pulls the correct icon for this probe type.
    public void RefreshIcon()
    {
        if (iconImage == null || ProbeManager.Instance == null)
            return;

        iconImage.sprite = ProbeManager.Instance.GetProbeIcon(type);
        iconImage.enabled = iconImage.sprite != null;
    }

    // Attempts to purchase a probe and refreshes UI if successful.
    public void OnClick()
    {
        Debug.Log($"Attempting to purchase probe of type {type}");
        var planet = MiningManager.Instance.CurrentPlanet;
        if (ProbeManager.Instance.PurchaseProbe(type, planet))
        {
            button.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // reset scale to ensure no drift
                gameObject.transform.localScale = Vector3.one;
            });

            Debug.Log($"Purchased probe of type {type} for planet {planet.planetName}");
            MiningManager.Instance.miningUI.RefreshPlanetUI(planet);
        }
        else
        {
            Debug.Log($"Failed to purchase probe of type {type} for planet {planet.planetName}");
            button.transform.DOPunchPosition(Vector3.one * 0.1f, 0.1f, 10, 1)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // reset position to ensure no drift
                gameObject.transform.localPosition = Vector3.zero;
            });
        }
    }

    // Updates the button visibility/icon based on global probe unlocks and affordability.
    public void RefreshState()
    {
        bool unlocked = ProbeManager.Instance.IsProbeUnlocked(type);
        gameObject.SetActive(unlocked);

        RefreshIcon();
    }

}
