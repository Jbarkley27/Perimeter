using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillTreeUIManager : MonoBehaviour
{
    public static SkillTreeUIManager Instance;

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


    public void CenterUIOnScreen()
    {
        // Implement centering logic if needed
        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
    }
    

    public void ShowSkillHover(SkillData skillData)
    {
        skillHoverCanvasGroup.alpha = 0f;
        skillHoverNameText.text = skillData.skillName;
        skillHoverDescriptionText.text = skillData.description;
        skillHoverElementImage.sprite = GlobalDataStore.Instance.SkillElementLibrary.GetElementIcon(skillData.element);
        costText.text = $"Cost: {skillData.cost} Glass";
        if (skillData.isPassive) 
            currentLevelText.text = $"Level: {skillData.currentLevel}/{skillData.maxLevel}";
        else
            currentLevelText.text = $"Unlocked";

        purchaseOrUpgradeText.text = !skillData.isPassive ? "Unlock" : "Upgrade";

        skillHoverCanvasGroup.DOFade(1f, 0.15f).SetEase(Ease.OutQuad);
    }


    public void HideSkillHover()
    {
        skillHoverCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutQuad);
    }

}