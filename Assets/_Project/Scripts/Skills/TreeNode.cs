using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TreeNode
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