using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class TreeNode: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SkillData skillData;
    // public TreeNode center;
    public List<TreeNode> children = new List<TreeNode>();
    public CanvasGroup connectionLine;
    public enum NodeState
    {
        Locked,
        Unlocked,

    }
    public NodeState nodeState = NodeState.Locked;

    [Header("UI Elements")]
    public Image nodeIcon;
    public Image nodeBackground;
    public Image nodeBorderImageRoot;
    public CanvasGroup nodeCanvasGroup;
    public CanvasGroup hoverHighlightCanvasGroup;
    public Image canAffordImage;
    public Color activatedColor = Color.gold;
    public SkillDraggable draggableComponent;
    public GameObject originalPosition;
    public Slider passiveLevelSlider;


    void Start()
    {
        if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(false);
        if (passiveLevelSlider != null)
        {
            if (skillData != null && skillData.isPassive)
            {
                passiveLevelSlider.gameObject.SetActive(true);
                passiveLevelSlider.maxValue = skillData.maxLevel;
                passiveLevelSlider.value = skillData.currentLevel;
            }
            else
            {
                passiveLevelSlider.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        UpdateNodeUI();

        if (draggableComponent != null)
        {
            // disable dragging for locked nodes or passive skills
            if (nodeState == NodeState.Locked || (skillData != null && skillData.isPassive))
            {
                draggableComponent.enabled = false;
            }
            else
            {
                draggableComponent.enabled = true;
            }

            // Disabled all hover effects while dragging
            if (draggableComponent.IsDragging)
            {
                if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(false);
                SkillTreeUIManager.Instance.HideSkillUIPanel();
            }
        }

        // turn off the canAffordImage if the skill slotted
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PurchaseOrUpgradeNode();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(true);
        if (skillData != null && !SkillLoadout.Instance.IsSkillEquipped(skillData)) SkillTreeUIManager.Instance.ShowSkillUIPanel(skillData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(false);
        SkillTreeUIManager.Instance.HideSkillUIPanel();
    }



    public void SetConnectionLineActive(bool isActive)
    {
        if (connectionLine != null)
        {
            connectionLine.alpha = isActive ? 1f : 0f;
        }
    }


    public void SetNodeState(NodeState newState)
    {
        nodeState = newState;
        UpdateNodeUI();
    }


    public void InitializeNode()
    {
        // Initialize children nodes
        foreach (var child in children)
        {
            child.InitializeNode();
        }

        // Set initial UI state
        UpdateNodeUI();

        // Set SkillData 
        if (skillData != null && nodeIcon != null)
        {
            nodeIcon.sprite = skillData.icon;
        }

        // change background color based on element
        if (skillData != null && nodeBackground != null)
        {
            nodeBackground.color = GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);
        }
    }

    public void UpdateNodeUI()
    {
        switch (nodeState)
        {
            case NodeState.Locked:
                nodeCanvasGroup.alpha = 0.95f;
                nodeBorderImageRoot.color = Color.gray;
                break;
            case NodeState.Unlocked:
                nodeCanvasGroup.alpha = 1f;
                nodeBorderImageRoot.color = Color.white;
                break;
        }

        // we dont want to show affordability for locked nodes, only
        // nodes that can be purchased or upgraded, in the final game
        if (canAffordImage != null)
        {
            if (draggableComponent != null && draggableComponent.IsSlotted
                || skillData.isUnlocked)
            {
                canAffordImage.gameObject.SetActive(false);
                return;
            }

            // Example logic: highlight if player has enough currency to unlock/upgrade
            bool canAfford = false;
            if (skillData != null)
            {
                canAfford = GlassManager.Instance.CanAffordNodePurchase(skillData.cost);
            }

            // Change color based on affordability
            if (canAfford)
            {
                canAffordImage.gameObject.SetActive(true);
            }
            else
            {
                canAffordImage.gameObject.SetActive(false);
            }
        }


        // Level slider
        if (skillData != null && skillData.isPassive == false && passiveLevelSlider != null)
        {
            passiveLevelSlider.gameObject.SetActive(false);
        }
        else if (passiveLevelSlider != null && skillData != null && skillData.isPassive)
        {
            passiveLevelSlider.gameObject.SetActive(true);
            passiveLevelSlider.value = skillData.currentLevel;
        }


    }


    public void PurchaseOrUpgradeNode()
    {
        if (nodeState == NodeState.Locked)
        {
            // Purchase logic
            if (GlassManager.Instance.SpendGlass(skillData.cost))
            {
                SetNodeState(NodeState.Unlocked);
                skillData.isUnlocked = true;
                Debug.Log($"Purchased skill node: {skillData.skillName}");
            }
            else
            {
                Debug.Log("Not enough Glass to purchase this skill.");
            }
        }
        else if (nodeState == NodeState.Unlocked && skillData.isPassive)
        {
            // Upgrade logic for passive skills
            if (skillData.currentLevel < skillData.maxLevel)
            {
                if (GlassManager.Instance.SpendGlass(skillData.cost))
                {
                    skillData.currentLevel += 1;
                    if (passiveLevelSlider != null)
                    {
                        passiveLevelSlider.value = skillData.currentLevel;
                        passiveLevelSlider.gameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1)
                            .SetEase(Ease.OutCubic)
                            .OnComplete(() =>
                            {
                                passiveLevelSlider.gameObject.transform.localScale = Vector3.one;
                            });
                    }
                    Debug.Log($"Upgraded skill node: {skillData.skillName} to level {skillData.currentLevel}");
                }
                else
                {
                    Debug.Log("Not enough Glass to upgrade this skill.");
                }
            }
            else
            {
                Debug.Log("Skill is already at max level.");
            }
        }

        SkillTreeUIManager.Instance.UpdateSkillUIPanel(skillData);


    }
}