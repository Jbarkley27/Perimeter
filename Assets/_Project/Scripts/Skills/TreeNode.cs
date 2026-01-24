using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        Active,
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


    void Start()
    {
        if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(false);
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
                canAffordImage.gameObject.SetActive(false);
            }
        }

        // turn off the canAffordImage if the skill slotted
        if (draggableComponent != null && draggableComponent.IsSlotted)
        {
            canAffordImage.gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // nothing for now
        // will add skill purchase logic later
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("GameObject Name: " + gameObject.name);
        if (nodeState != NodeState.Locked)
        {
            Debug.Log($"Pointer Entered Node: {skillData.skillName}");
            if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(true);
            SkillTreeUIManager.Instance.ShowSkillUIPanel(skillData);
        }

        // add logic to show a locked skill info later
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
                nodeCanvasGroup.alpha = 0.8f;
                nodeBorderImageRoot.color = Color.gray;
                break;
            case NodeState.Unlocked:
                nodeCanvasGroup.alpha = 0.98f;
                nodeBorderImageRoot.color = Color.lightGray;
                break;
            case NodeState.Active:
                nodeCanvasGroup.alpha = 1f;
                nodeBorderImageRoot.color = activatedColor;
                break;
        }

        // we dont want to show affordability for locked nodes, only
        // nodes that can be purchased or upgraded, in the final game
        // locked nodes may not even exist in the skill tree UI
        // TODO: why show the player something they cant interact with?
        if (canAffordImage != null)
        {
            if (nodeState == NodeState.Locked && draggableComponent != null && !draggableComponent.IsSlotted)
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


        // update background color based on 
    }


    public void PurchaseOrUpgradeNode()
    {
        if (nodeState == NodeState.Locked)
        {
            // Purchase logic
            if (GlassManager.Instance.SpendGlass(skillData.cost))
            {
                SetNodeState(NodeState.Unlocked);
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
    }
}