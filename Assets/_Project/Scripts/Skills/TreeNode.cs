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
        Active
    }
    public NodeState nodeState = NodeState.Locked;

    [Header("UI Elements")]
    public Image nodeIcon;
    public Image nodeBackground;
    public CanvasGroup nodeCanvasGroup;
    public CanvasGroup hoverHighlightCanvasGroup;


    public void OnPointerClick(PointerEventData eventData)
    {
        // nothing for now
        // will add skill purchase logic later
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (nodeState != NodeState.Locked)
        {
            hoverHighlightCanvasGroup.alpha = 1f;
            SkillTreeUIManager.Instance.ShowSkillUIPanel(skillData);
        }

        // add logic to show a locked skill info later
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverHighlightCanvasGroup.alpha = 0f;
        SkillTreeUIManager.Instance.HideSkillUIPanel();
    }



    public void SetConnectionLineActive(bool isActive)
    {
        if (connectionLine != null)
        {
            connectionLine.alpha = isActive ? 1f : 0f;
        }
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
                nodeCanvasGroup.alpha = 0.3f;
                break;
            case NodeState.Unlocked:
                nodeCanvasGroup.alpha = 0.8f;
                break;
            case NodeState.Active:
                nodeCanvasGroup.alpha = 1f;
                break;
        }
    }
}