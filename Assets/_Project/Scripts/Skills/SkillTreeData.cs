using System.Collections.Generic;
using UnityEngine;

// Singleton class to manage the skill tree data, non-UI related

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

    void Start()
    {
        InitializeTree();
    }

    public void InitializeTree()
    {
        if (rootNode != null)
        {
            rootNode.InitializeNode();
        }
    }
}