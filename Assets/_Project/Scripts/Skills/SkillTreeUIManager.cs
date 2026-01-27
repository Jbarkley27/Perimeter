using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillTreeUIManager : MonoBehaviour
{
    public static SkillTreeUIManager Instance;
    public bool isHovering = false;
    public ScrollRect scrollRect;
    public SkillData hoveredSkillData;
    public TreeNode hoveredNode;
    
    // Skill Hover For More Info
    [Header("Skill Hover UI Elements")]
    public CanvasGroup skillHoverCanvasGroup;
    public TMP_Text skillHoverNameText;
    public TMP_Text skillHoverDescriptionText;
    public Image skillHoverElementImage;
    public TMP_Text costText;
    public TMP_Text currentLevelText;
    public TMP_Text purchaseOrUpgradeText;
    public ScrollRect parentScrollRect;
    public GameObject skillLoadoutIcon;
    public Button purchaseOrUpgradeButton;
    public Color canAffordColor = Color.white;
    public Color cannotAffordColor = Color.red;
    public TMP_Text nextUpgradeText;
    public TreeNodeConnector nodeConnectorPrefab;
    public RectTransform connectorParent;


    [Header("Follow Mouse Settings")]
    public Vector3 mouseOffset = new Vector3(15f, -15f, 0f);
    public Vector3 equippedOffset = new Vector3(15f, -45f, 0f);


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found a SkillTreeUIManager object, destroying new one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        HideSkillUIPanel();
        CenterUIOnScreen();
    }

    void Update()
    {
        if (isHovering && hoveredNode != null)
        {
            FollowMousePosition(WorldCursor.instance.GetCursorPosition());
            UpdateSkillUIPanel(hoveredNode);
        }
    }



    public void CenterUIOnScreen()
    {
        // Implement centering logic if needed
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }


    
    public void ShowSkillUIPanel(TreeNode node)
    {
        if (node == null || node.skillData == null) return;

        hoveredNode = node;
        hoveredSkillData = node.skillData;
        SkillData skillData = node.skillData;

        isHovering = true;

        skillHoverCanvasGroup.DOKill();

        // Populate UI Elements
        skillHoverCanvasGroup.alpha = 0f;
        skillHoverNameText.text = skillData.skillName;




        // Set description with current effects
        string desc = string.IsNullOrWhiteSpace(skillData.description) ? "" : skillData.description;

        if (skillData.isPassive)
        {
            string currentLine = BuildEffectsLine(skillData, skillData.currentLevel, true);
            if (!string.IsNullOrEmpty(currentLine))
                desc += (desc.Length > 0 ? "\n" : "") + $"Current: {currentLine}";

            if (skillData.currentLevel < skillData.maxLevel)
            {
                string nextLine = BuildEffectsLine(skillData, skillData.currentLevel + 1, false);
                if (!string.IsNullOrEmpty(nextLine))
                    desc += (desc.Length > 0 ? "\n" : "") + $"Next: {nextLine}";
            }
            else
            {
                desc += (desc.Length > 0 ? "\n" : "") + "Max Level";
            }
        }

        skillHoverDescriptionText.text = desc;


        UpdateHoverDescriptionText(skillData);


        skillHoverElementImage.sprite =
            GlobalDataStore.Instance.SkillElementLibrary.GetElementIcon(skillData.element);

        skillHoverElementImage.color =
            GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);

        costText.text = $"Cost: {skillData.cost} Glass";

        bool isAvailable = node.IsAvailable();
        bool isActive = SkillTreeData.Instance.IsNodeActive(node);
        bool isExclusive = !string.IsNullOrEmpty(skillData.exclusiveGroupId);
        bool isPassive = skillData.isPassive;
        bool isMaxLevel = isPassive && skillData.currentLevel >= skillData.maxLevel;
        bool isExclusiveMax = isExclusive && skillData.currentLevel >= skillData.maxLevel;

        // if (nextUpgradeText != null)
        // {
        //     if (skillData.isPassive && skillData.currentLevel < skillData.maxLevel)
        //     {
        //         nextUpgradeText.gameObject.SetActive(true);
        //         nextUpgradeText.text = $"Next: {BuildNextUpgradeDescription(skillData)}";
        //     }
        //     else
        //     {
        //         nextUpgradeText.gameObject.SetActive(false);
        //     }
        // }

        if (!isAvailable)
        {
            currentLevelText.text = "Locked";
            purchaseOrUpgradeButton.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
        }
        else
        {
            costText.gameObject.SetActive(true);

            if (isPassive)
                currentLevelText.text = $"Level: {skillData.currentLevel}/{skillData.maxLevel}";
            else if (isExclusive)
                currentLevelText.text = skillData.isExclusiveActive ? "Active" : "Inactive";
            else
                currentLevelText.text = isActive ? "Active" : "Unlocked";

            if (isExclusive)
            {
                purchaseOrUpgradeButton.gameObject.SetActive(true);

                if (isExclusiveMax)
                    costText.gameObject.SetActive(false);

                purchaseOrUpgradeButton.interactable = !skillData.isExclusiveActive;
                purchaseOrUpgradeText.text = skillData.isExclusiveActive ? "Active" : "Activate";
                purchaseOrUpgradeButton.GetComponent<Image>().color =
                    purchaseOrUpgradeButton.interactable ? canAffordColor : cannotAffordColor;
            }
            else if (isPassive)
            {
                purchaseOrUpgradeButton.gameObject.SetActive(true);

                if (isMaxLevel)
                {
                    purchaseOrUpgradeButton.interactable = false;
                    purchaseOrUpgradeText.text = "Max Level";
                    purchaseOrUpgradeButton.GetComponent<Image>().color = cannotAffordColor;
                    costText.gameObject.SetActive(false);
                }
                else if (GlassManager.Instance.CanAffordNodePurchase(skillData.cost))
                {
                    purchaseOrUpgradeButton.interactable = true;
                    purchaseOrUpgradeButton.GetComponent<Image>().color = canAffordColor;
                    purchaseOrUpgradeText.text = "Upgrade";
                }
                else
                {
                    purchaseOrUpgradeButton.interactable = false;
                    purchaseOrUpgradeButton.GetComponent<Image>().color = cannotAffordColor;
                    purchaseOrUpgradeText.text = "Cannot Afford";
                }
            }
            else
            {
                if (skillData.isUnlocked)
                {
                    purchaseOrUpgradeButton.gameObject.SetActive(false);
                    costText.gameObject.SetActive(false);
                }
                else
                {
                    purchaseOrUpgradeButton.gameObject.SetActive(true);
                    bool canAfford = GlassManager.Instance.CanAffordNodePurchase(skillData.cost);
                    purchaseOrUpgradeButton.interactable = canAfford;
                    purchaseOrUpgradeButton.GetComponent<Image>().color = canAfford ? canAffordColor : cannotAffordColor;
                    purchaseOrUpgradeText.text = canAfford ? "Unlock" : "Cannot Afford";
                }
            }
        }

        skillHoverCanvasGroup.gameObject.SetActive(true);
        skillHoverCanvasGroup
            .DOFade(1f, 0.15f)
            .SetEase(Ease.OutQuad);
    }



    public void UpdateSkillUIPanel(TreeNode node)
    {
        if (!isHovering || node == null || node.skillData == null) return;

        SkillData skillData = node.skillData;

        bool isAvailable = node.IsAvailable();
        bool isActive = SkillTreeData.Instance.IsNodeActive(node);
        bool isExclusive = !string.IsNullOrEmpty(skillData.exclusiveGroupId);
        bool isPassive = skillData.isPassive;
        bool isMaxLevel = isPassive && skillData.currentLevel >= skillData.maxLevel;
        bool isExclusiveMax = isExclusive && skillData.currentLevel >= skillData.maxLevel;


        if (!isAvailable)
        {
            currentLevelText.text = "Locked";
            purchaseOrUpgradeButton.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
            return;
        }

        costText.gameObject.SetActive(true);

        skillHoverElementImage.sprite =
            GlobalDataStore.Instance.SkillElementLibrary.GetElementIcon(skillData.element);
        skillHoverElementImage.color =
            GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);


        UpdateHoverDescriptionText(skillData);


        if (isPassive)
            currentLevelText.text = $"Level: {skillData.currentLevel}/{skillData.maxLevel}";
        else if (isExclusive)
            currentLevelText.text = skillData.isExclusiveActive ? "Active" : "Inactive";
        else
            currentLevelText.text = isActive ? "Active" : "Unlocked";

        if (isExclusive)
        {
            purchaseOrUpgradeButton.gameObject.SetActive(true);
            purchaseOrUpgradeButton.interactable = !skillData.isExclusiveActive;
            purchaseOrUpgradeText.text = skillData.isExclusiveActive ? "Active" : "Activate";
            purchaseOrUpgradeButton.GetComponent<Image>().color =
                purchaseOrUpgradeButton.interactable ? canAffordColor : cannotAffordColor;

            if (isExclusiveMax)
                costText.gameObject.SetActive(false);

            return;
        }

        if (isPassive)
        {
            purchaseOrUpgradeButton.gameObject.SetActive(true);

            if (isMaxLevel)
            {
                purchaseOrUpgradeButton.interactable = false;
                purchaseOrUpgradeText.text = "Max Level";
                purchaseOrUpgradeButton.GetComponent<Image>().color = cannotAffordColor;
                costText.gameObject.SetActive(false);
            }
            else if (GlassManager.Instance.CanAffordNodePurchase(skillData.cost))
            {
                purchaseOrUpgradeButton.interactable = true;
                purchaseOrUpgradeButton.GetComponent<Image>().color = canAffordColor;
                purchaseOrUpgradeText.text = "Upgrade";
            }
            else
            {
                purchaseOrUpgradeButton.interactable = false;
                purchaseOrUpgradeButton.GetComponent<Image>().color = cannotAffordColor;
                purchaseOrUpgradeText.text = "Cannot Afford";
            }

            return;
        }

        // Active (non-passive) skill
        if (skillData.isUnlocked)
        {
            purchaseOrUpgradeButton.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
        }
        else
        {
            bool canAfford = GlassManager.Instance.CanAffordNodePurchase(skillData.cost);
            purchaseOrUpgradeButton.gameObject.SetActive(true);
            purchaseOrUpgradeButton.interactable = canAfford;
            purchaseOrUpgradeButton.GetComponent<Image>().color = canAfford ? canAffordColor : cannotAffordColor;
            purchaseOrUpgradeText.text = canAfford ? "Unlock" : "Cannot Afford";
        }
    }


    private string BuildEffectsLine(SkillData skillData, int level, bool includeTotals)
    {
        if (skillData == null || skillData.upgrades == null)
            return string.Empty;

        SkillUpgrade upgrade = null;
        foreach (var u in skillData.upgrades)
        {
            if (u != null && u.level == level)
            {
                upgrade = u;
                break;
            }
        }

        if (upgrade == null || upgrade.effects == null || upgrade.effects.Count == 0)
            return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var effect in upgrade.effects)
        {
            if (effect == null) continue;

            if (sb.Length > 0) sb.Append(", ");
            sb.Append(includeTotals ? effect.GetDescriptionWithValue() : effect.GetDescription());
        }

        return sb.ToString();
    }




    private void UpdateHoverDescriptionText(SkillData skillData)
    {
        string desc = string.IsNullOrWhiteSpace(skillData.description) ? "" : skillData.description;

        if (skillData.isPassive)
        {
            string currentLine = BuildEffectsLine(skillData, skillData.currentLevel, true);
            if (!string.IsNullOrEmpty(currentLine))
                desc += (desc.Length > 0 ? "\n" : "") + $"Current: {currentLine}";

            if (skillData.currentLevel < skillData.maxLevel)
            {
                string nextLine = BuildEffectsLine(skillData, skillData.currentLevel + 1, false);
                if (!string.IsNullOrEmpty(nextLine))
                    desc += (desc.Length > 0 ? "\n" : "") + $"Next: {nextLine}";
            }
            else
            {
                desc += (desc.Length > 0 ? "\n" : "") + "Max Level";
            }
        }

        skillHoverDescriptionText.text = desc;
    }



    public void HideSkillUIPanel()
    {
        isHovering = false;

        skillHoverCanvasGroup.DOKill();

        skillHoverCanvasGroup
            .DOFade(0f, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Prevent old tweens from hiding a newly shown panel
                if (!isHovering)
                    skillHoverCanvasGroup.gameObject.SetActive(false);
            });
    }



    public void FollowMousePosition(Vector3 mousePosition)
    {
        if (!isHovering) return;

        Vector3 targetPosition = mousePosition + (SkillLoadout.Instance.IsSkillEquipped(hoveredSkillData)
            ? equippedOffset
            : mouseOffset);
        skillHoverCanvasGroup.gameObject.transform.position = targetPosition;
    }


}