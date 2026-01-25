using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class TreeNode: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SkillData skillData;
    public List<TreeNode> children = new List<TreeNode>();
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
    public SkillDraggable draggableComponent;
    public Slider passiveLevelSlider;


    void Start()
    {
        // Turn off hover highlight at start
        if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(false);

        
    }



    void Update()
    {
        UpdateNodeUI();
        ManageDraggableState();
    }





    public void ManageDraggableState()
    {
        // Manage draggable state
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

    }


    #region Pointer Events

    public void OnPointerClick(PointerEventData eventData)
    {
        PurchaseOrUpgradeNode();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(true);
        if (skillData != null) SkillTreeUIManager.Instance.ShowSkillUIPanel(skillData);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(false);
        SkillTreeUIManager.Instance.HideSkillUIPanel();
    }

    #endregion



    public void SetNodeState(NodeState newState)
    {
        nodeState = newState;

        // Update skill data accordingly
        if (skillData != null)
        {
            skillData.isUnlocked = (newState == NodeState.Unlocked);
        }

        UpdateNodeUI();
    }




    // Initialize node and its children
    public void InitializeNode()
    {
        // Initialize children nodes
        foreach (var child in children)
        {
            child.InitializeNode();
        }

        // Sync node state with saved skill data on startup
        // This ensures the UI reflects the correct state since 
        // skills are scriptable objects that persist between sessions
        if (skillData != null)
        {
            nodeState = skillData.isUnlocked ? NodeState.Unlocked : NodeState.Locked;
        }


        // Set Node Icon
        if (skillData != null && nodeIcon != null)
        {
            nodeIcon.sprite = skillData.icon;
        }

        // change background color based on element
        if (skillData != null && nodeBackground != null)
        {
            nodeBackground.color = GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);
        }

        // Update slider max value for passive skills
        // Setup passive level slider
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



    // Update the visual state of the node based on its current state
    public void UpdateNodeUI()
    {
        if (skillData == null)
            return;


        // Update the locked/unlocked visual state
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


        // Level slider for passive skills
        // hide for active skills
        if (skillData != null && skillData.isPassive == false && passiveLevelSlider != null)
        {
            passiveLevelSlider.gameObject.SetActive(false);
        }

        // show and update for passive skills
        else if (passiveLevelSlider != null && skillData != null && skillData.isPassive)
        {
            passiveLevelSlider.gameObject.SetActive(true);
            passiveLevelSlider.value = skillData.currentLevel;
        }



        // AFFORDABILITY INDICATOR LOGIC
        // we dont want to show affordability for locked nodes, only
        // nodes that can be purchased or upgraded, in the final game
        if (canAffordImage != null)
        {
            // split logic for passive vs active skills
            if (skillData.isPassive)
            {
                // Passive skill logic: only show if not max level
                if (skillData.currentLevel >= skillData.maxLevel)
                {
                    canAffordImage.gameObject.SetActive(false);
                    return;
                }
            }
            else
            {
                // Active skill logic: only show if not unlocked
                if (skillData.isUnlocked)
                {
                    canAffordImage.gameObject.SetActive(false);
                    return;
                }
            }




            // This check should only run if the node is purchasable or upgradable
            // Now we check if the player can afford it
            bool canAfford = false;

            // Check affordability
            if (skillData != null)
            {
                canAfford = GlassManager.Instance.CanAffordNodePurchase(skillData.cost);
            }

            // Change visibility based on affordability
            if (canAfford)
            {
                canAffordImage.gameObject.SetActive(true);
            }
            else
            {
                canAffordImage.gameObject.SetActive(false);
            }
        }
    }




    public void PurchaseOrUpgradeNode()
    {
        // IF ACTIVE, HANDLE UPGRADE LOGIC
        if (!skillData.isPassive)
        {
            // Already unlocked
            if (skillData.isUnlocked)
            {
                Debug.Log("Skill already unlocked.");
                return;
            }
            
            // Purchase logic
            if (GlassManager.Instance.SpendGlass(skillData.cost))
            {
                SetNodeState(NodeState.Unlocked);
                gameObject.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1, 0.5f)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() =>
                    {
                        gameObject.transform.localScale = Vector3.one;
                    });
                Debug.Log($"Purchased skill node: {skillData.skillName}");
            }
            else
            {
                Debug.Log("Not enough Glass to purchase this skill.");
            }
        }

        // IF PASSIVE, HANDLE UPGRADE LOGIC
        else if (skillData.isPassive)
        {
            if (!skillData.isUnlocked)
            {
                // Unlock then allow the first upgrade
                // This ensures when the player first unlocks the passive skill,
                // it is marked as unlocked in the skill data and reaches level 1
                SetNodeState(NodeState.Unlocked);
            }


            // Upgrade logic for passive skills
            if (skillData.currentLevel < skillData.maxLevel)
            {
                // If we can afford the upgrade, then purchase it and increase level
                if (GlassManager.Instance.SpendGlass(skillData.cost))
                {
                    IncreaseSkillLevel();
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
    }

    public void IncreaseSkillLevel()
    {
        if (!skillData.isPassive || skillData.currentLevel >= skillData.maxLevel)
            return;
        
        skillData.currentLevel += 1;

        passiveLevelSlider.gameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1)
                            .SetEase(Ease.OutCubic)
                            .OnComplete(() =>
                            {
                                passiveLevelSlider.gameObject.transform.localScale = Vector3.one;
                            });

        Debug.Log($"Upgraded skill node: {skillData.skillName} to level {skillData.currentLevel}");
    }
}