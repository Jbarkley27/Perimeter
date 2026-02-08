using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * SkillNodeInfoPanel
 * ------------------
 * Hover panel for a skill node definition + runtime state.
 */
public class SkillNodeInfoPanel : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text levelText;
    public TMP_Text costText;
    public TMP_Text statusText;

    [Header("Element")]
    public Image elementImage;
    public Color affordableColor = Color.white;
    public Color unaffordableColor = Color.red;
    public Image buyButtonImage;



    [Header("Follow Mouse")]
    public Canvas rootCanvas;
    public Vector2 mouseOffset = new Vector2(15f, -15f);
    public Vector2 altOffset = new Vector2(-15f, -15f);
    public bool useAltOffset = false;

    private RectTransform panelRect;
    private RectTransform parentRect;

    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        parentRect = panelRect != null ? panelRect.parent as RectTransform : null;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (!gameObject.activeSelf || panelRect == null || parentRect == null)
            return;

        Vector3 cursor = WorldCursor.instance != null
            ? WorldCursor.instance.GetCursorPosition()
            : Input.mousePosition;

        Camera cam = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, cursor, cam, out Vector2 local))
            panelRect.anchoredPosition = local + (useAltOffset ? altOffset : mouseOffset);
    }


    // Shows the panel with current data.
    public void Show(SkillNodeDefinition node, SkillTreeRuntime runtime, bool altOffset = false)
    {
        useAltOffset = altOffset;
        Refresh(node, runtime);
        gameObject.SetActive(true);
        gameObject.GetComponent<CanvasGroup>().alpha = 1;
    }

    // Hides the panel.
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Refreshes all UI fields (call when hovering or after state changes).
    public void Refresh(SkillNodeDefinition node, SkillTreeRuntime runtime)
    {
        if (node == null || runtime == null)
            return;

        int level = runtime.GetLevel(node);
        bool isAvailable = runtime.IsAvailable(node);
        bool isUnlocked = level > 0;
        bool isExclusive = node.IsExclusive;
        bool isExclusiveActive = runtime.IsExclusiveActive(node);

        if (nameText != null)
            nameText.text = node.displayName;

        if (descriptionText != null)
        {
            string desc = node.description;

            // Add current/next effect lines for passive nodes.
            if (node.IsPassive)
            {
                string currentLine = BuildEffectsLine(node, level, true);
                if (!string.IsNullOrEmpty(currentLine))
                    desc += (desc.Length > 0 ? "\n" : "") + $"Current: {currentLine}";

                if (level < node.maxLevel)
                {
                    string nextLine = BuildEffectsLine(node, level + 1, false);
                    if (!string.IsNullOrEmpty(nextLine))
                        desc += (desc.Length > 0 ? "\n" : "") + $"Next: {nextLine}";
                }
                else
                {
                    desc += (desc.Length > 0 ? "\n" : "") + "Max Level";
                }
            }

            descriptionText.text = desc;
        }


        if (levelText != null && statusText != null)
        {
            if (!isAvailable)
            {
                levelText.text = "Hidden";
                statusText.text = "";
            }
            else if (SkillTreeRuntime.Instance.IsAtMaxLevel(node))
            {
                levelText.text = $"Max Level";
                statusText.text = "";
            }
            else if (!SkillTreeRuntime.Instance.IsAtMaxLevel(node))
            {
                levelText.text = $"Level {level}/{node.maxLevel}";
                statusText.text = node.IsPassive ? "Upgrade" : "Unlock";
            }
            else
                levelText.text = $"N/A";
        }


        // Change color of buy button based on affordability
        if (buyButtonImage != null)        {
            if (!isAvailable)
                buyButtonImage.color = Color.gray;
            else
            {
                int cost = node.IsPassive
                    ? runtime.GetCostForNextLevel(node)
                    : runtime.GetCostForLevel(node, 1);

                buyButtonImage.color = GlassManager.Instance != null && GlassManager.Instance.GetTotalGlassShardsCollected() >= cost
                    ? affordableColor
                    : unaffordableColor;
            }
        }



        if (costText != null)
        {
            if (!isAvailable)
            {
                costText.text = "";
            }
            else if (node.IsExclusive)
            {
                costText.text = isUnlocked ? "Switch: Free" : $"Unlock: {runtime.GetCostForLevel(node, 1)}";
            }
            else if (node.IsPassive)
            {
                if (level >= node.maxLevel)
                    costText.text = "Max Level";
                else
                    costText.text = $"Upgrade: {runtime.GetCostForNextLevel(node)}";
            }
            else
            {
                costText.text = isUnlocked ? "Unlocked" : $"Unlock: {runtime.GetCostForLevel(node, 1)}";
            }
        }




        if (elementImage != null)
        {
            elementImage.sprite = GlobalDataStore.Instance.SkillElementLibrary.GetElementIcon(node.element);
            elementImage.color = GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(node.element);
        }
    }

    // Builds a comma‑separated list of effects for a given level.
    private string BuildEffectsLine(SkillNodeDefinition node, int level, bool includeTotals)
    {
        if (node == null || node.levelEffects == null)
            return string.Empty;

        SkillNodeLevelEffects entry = default;
        bool found = false;

        for (int i = 0; i < node.levelEffects.Count; i++)
        {
            if (node.levelEffects[i].level == level)
            {
                entry = node.levelEffects[i];
                found = true;
                break;
            }
        }

        if (!found || entry.effects == null || entry.effects.Count == 0)
            return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var effect in entry.effects)
        {
            if (effect == null) continue;
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(includeTotals ? effect.GetDescriptionWithValue() : effect.GetDescription());
        }

        return sb.ToString();
    }
}
