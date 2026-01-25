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
        if (isHovering)
        {
            FollowMousePosition(WorldCursor.instance.GetCursorPosition());

            if (hoveredSkillData != null)
            {
                UpdateSkillUIPanel(hoveredSkillData);
            }
        }


    }



    public void CenterUIOnScreen()
    {
        // Implement centering logic if needed
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }


    
    public void ShowSkillUIPanel(SkillData skillData)
    {
        if (skillData == null) return;

        hoveredSkillData = skillData;

        isHovering = true;

        skillHoverCanvasGroup.DOKill();

        // Populate UI Elements
        skillHoverCanvasGroup.alpha = 0f;
        skillHoverNameText.text = skillData.skillName;
        skillHoverDescriptionText.text = skillData.description;
        skillHoverElementImage.sprite =
            GlobalDataStore.Instance.SkillElementLibrary.GetElementIcon(skillData.element);

        skillHoverElementImage.color =
            GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);

        costText.text = $"Cost: {skillData.cost} Glass";


        // Handle Current Level Text
        if (skillData.isPassive)
            currentLevelText.text = $"Level: {skillData.currentLevel}/{skillData.maxLevel}";
        else
        {
            if (SkillLoadout.Instance.IsSkillEquipped(skillData))
            {
                currentLevelText.text = $"Equipped";
            }
            else if (skillData.isUnlocked)
            {
                currentLevelText.text = $"Unlocked";
            }
            else
            {
                currentLevelText.text = $"Locked";
            }
        }

    


        // UPGRADE/PURCHASE BUTTON
        // If its equipped or unlocked (for non-passive), hide the button
        if (SkillLoadout.Instance.IsSkillEquipped(skillData)
            || (!skillData.isPassive && skillData.isUnlocked))
        {
            // since it's already equipped or unlocked, hide the button
            purchaseOrUpgradeButton.gameObject.SetActive(false);
        }
        // Otherwise, show the button
        else
        {
            purchaseOrUpgradeButton.gameObject.SetActive(true);
        }


        if (GlassManager.Instance.CanAffordNodePurchase(skillData.cost) &&
            (skillData.isPassive ? skillData.currentLevel < skillData.maxLevel : true))
        {
            purchaseOrUpgradeButton.interactable = true;
            purchaseOrUpgradeButton.GetComponent<Image>().color = canAffordColor;
            purchaseOrUpgradeText.text = !skillData.isPassive ? "Unlock" : "Upgrade";
        }
        else
        {
            purchaseOrUpgradeButton.interactable = false;
            purchaseOrUpgradeButton.GetComponent<Image>().color = cannotAffordColor;
            purchaseOrUpgradeText.text = skillData.isPassive && skillData.currentLevel >= skillData.maxLevel ? "Max Level" : "Cannot Afford";
        }


        // Show skill hover panel
        skillHoverCanvasGroup.gameObject.SetActive(true);
        skillHoverCanvasGroup
            .DOFade(1f, 0.15f)
            .SetEase(Ease.OutQuad);
    }


    public void UpdateSkillUIPanel(SkillData skillData)
    {
        if (!isHovering) return;

        if(skillData.currentLevel >= skillData.maxLevel && skillData.isPassive)
        {
            purchaseOrUpgradeButton.interactable = false;
            purchaseOrUpgradeText.text = "Max Level";
            costText.gameObject.SetActive(false);
            currentLevelText.text = $"Level: {skillData.currentLevel}/{skillData.maxLevel}";
            return;
        }

        costText.gameObject.SetActive(true);

        if (skillData.isPassive)
            currentLevelText.text = $"Level: {skillData.currentLevel}/{skillData.maxLevel}";
        else
            currentLevelText.text = $"Unlocked";

        // update button
        if (SkillLoadout.Instance.IsSkillEquipped(skillData)
            || (!skillData.isPassive && skillData.isUnlocked))
        {
            purchaseOrUpgradeButton.gameObject.SetActive(false);
        }
        else
        {
            purchaseOrUpgradeButton.gameObject.SetActive(true);
        }
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