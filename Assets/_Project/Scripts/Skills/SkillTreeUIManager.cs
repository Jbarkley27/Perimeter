using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillTreeUIManager : MonoBehaviour
{
    public static SkillTreeUIManager Instance;
    public bool isHovering = false;
    public ScrollRect scrollRect;
    
    // Skill Hover For More Info
    [Header("Skill Hover UI Elements")]
    public CanvasGroup skillHoverCanvasGroup;
    public TMP_Text skillHoverNameText;
    public TMP_Text skillHoverDescriptionText;
    public Image skillHoverElementImage;
    public TMP_Text costText;
    public TMP_Text equippedText;
    public TMP_Text currentLevelText;
    public TMP_Text purchaseOrUpgradeText;


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

    public void CenterUIOnScreen()
    {
        // Implement centering logic if needed
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }
    
public void ShowSkillUIPanel(SkillData skillData)
{
    isHovering = true;

    skillHoverCanvasGroup.DOKill();

    skillHoverCanvasGroup.alpha = 0f;
    skillHoverNameText.text = skillData.skillName;
    skillHoverDescriptionText.text = skillData.description;
    skillHoverElementImage.sprite =
        GlobalDataStore.Instance.SkillElementLibrary.GetElementIcon(skillData.element);

    costText.text = $"Cost: {skillData.cost} Glass";

    if (skillData.isPassive)
        currentLevelText.text = $"Level: {skillData.currentLevel}/{skillData.maxLevel}";
    else
        currentLevelText.text = $"Unlocked";

    purchaseOrUpgradeText.text = !skillData.isPassive ? "Unlock" : "Upgrade";

    skillHoverCanvasGroup.gameObject.SetActive(true);

    skillHoverCanvasGroup
        .DOFade(1f, 0.15f)
        .SetEase(Ease.OutQuad);
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

}