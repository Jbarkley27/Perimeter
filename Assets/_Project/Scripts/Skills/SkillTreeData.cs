using System.Collections.Generic;
using UnityEngine;

public class SkillTreeData : MonoBehaviour
{
    public static SkillTreeData Instance;
    public TreeNode rootNode;
    public List<TreeNode> allNodes = new List<TreeNode>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeTree()
    {
        if (rootNode != null)
        {
            rootNode.InitializeNode();
        }
    }
}