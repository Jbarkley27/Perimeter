using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * ProbePurchaseHoverPanel
 * -----------------------
 * Display-only hover panel for probe purchase buttons.
 * Shows name, level 1, description, and a visual Deploy button with cost text.
 */
public class ProbePurchaseHoverPanel : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text descriptionText;
    public TMP_Text deployCostText;
    public Button deployButton;
    public CanvasGroup canvasGroup;

    // Populates the panel with purchase info.
    public void Show(string name, string description, string costText)
    {
        if (nameText != null)
            nameText.text = name;

        if (levelText != null)
            levelText.text = "Level 1";

        if (descriptionText != null)
            descriptionText.text = description;

        if (deployCostText != null)
            deployCostText.text = costText;

        // Display-only button (actual purchase happens on the UI node).
        if (deployButton != null)
            deployButton.interactable = false;

        // Intentionally do not modify panel canvas group alpha.

        gameObject.SetActive(true);
    }

    // Hides the panel.
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
