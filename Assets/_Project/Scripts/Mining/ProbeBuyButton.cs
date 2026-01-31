using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// Handles purchase button behavior for a specific probe type.
public class ProbeBuyButton : MonoBehaviour
{
    public ProbeType type;
    public Image iconImage;
    public Button button;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
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
            gameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // reset scale to ensure no drift
                gameObject.transform.localScale = Vector3.one;
            });
            
            Debug.Log($"Purchased probe of type {type} for planet {planet.planetName}");
            MiningManager.Instance.miningUI.RefreshPlanetUI(planet);
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
