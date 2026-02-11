using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 * SkillTreeUIController
 * ---------------------
 * Manages UI nodes, hover panel, and auto‑generated connectors.
 */

 [ExecuteAlways]
public class SkillTreeUIController : MonoBehaviour
{
    [Header("Runtime")]
    public SkillTreeRuntime runtime;

    [Header("Nodes")]
    public List<SkillNodeUI> nodes = new List<SkillNodeUI>();

    [Header("Connectors")]
    public TreeNodeConnector connectorPrefab;
    public RectTransform connectorParent;

    [Header("Hover Panel")]
    public SkillNodeInfoPanel infoPanel;
    // public Vector3 mouseOffset = new Vector3(15f, -15f, 0f);
    public Canvas rootCanvas;

    private SkillNodeUI hoveredNode;
    private RectTransform infoPanelRect;
    private readonly Dictionary<SkillNodeDefinition, SkillNodeUI> nodeLookup = new Dictionary<SkillNodeDefinition, SkillNodeUI>();
    private readonly List<TreeNodeConnector> spawnedConnectors = new List<TreeNodeConnector>();


    [Header("Layout")]
    public SkillTreeLayoutDefinition layoutDefinition;
    public SkillNodeUI nodePrefab;
    public RectTransform nodeParent;


    [Header("Editor Preview")]
    public bool previewLayoutInEditor = true;
    [Range(0f, 1f)] public float editorNodeGhostAlpha = 0.45f;
    [Range(0f, 1f)] public float editorConnectorGhostAlpha = 0.25f;

    #if UNITY_EDITOR
    private bool editorPreviewRebuildQueued;
    #endif


    


    private void Awake()
    {
        if (!Application.isPlaying)
        {
            QueueEditorPreviewRebuild();
            return;
        }

        if (runtime == null)
            runtime = SkillTreeRuntime.Instance;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (infoPanel != null)
            infoPanelRect = infoPanel.GetComponent<RectTransform>();

        CollectNodes();
        BuildLookup();
        BuildConnectors();
        RefreshAll();
        BuildFromLayout();
    }

    private void OnEnable()
    {
    #if UNITY_EDITOR
        SkillTreeLayoutDefinition.LayoutChanged += OnLayoutDefinitionChanged;
    #endif
        QueueEditorPreviewRebuild();
    }

    private void OnDisable()
    {
    #if UNITY_EDITOR
        SkillTreeLayoutDefinition.LayoutChanged -= OnLayoutDefinitionChanged;
    #endif
    }

    private void OnValidate()
    {
        QueueEditorPreviewRebuild();
    }


    private void Update()
    {
        // if (hoveredNode != null && infoPanel != null && infoPanel.gameObject.activeSelf)
        // {
        //     FollowMousePosition(GetCursorPosition());
        //     infoPanel.Refresh(hoveredNode.nodeDefinition, runtime);
        // }
    }

    // Collects nodes from children if list is empty.
    private void CollectNodes()
    {
        if (nodes != null && nodes.Count > 0)
            return;

        nodes = new List<SkillNodeUI>(GetComponentsInChildren<SkillNodeUI>(true));
    }

    // Builds the definition → node lookup.
    private void BuildLookup()
    {
        nodeLookup.Clear();

        for (int i = 0; i < nodes.Count; i++)
        {
            SkillNodeUI node = nodes[i];
            if (node == null || node.nodeDefinition == null)
                continue;

            node.uiController = this;
            nodeLookup[node.nodeDefinition] = node;
        }
    }


    // Regenerates all connectors based on prerequisites.
    public void BuildConnectors()
    {
        ClearConnectors();

        if (connectorPrefab == null || connectorParent == null)
            return;

        for (int i = 0; i < nodes.Count; i++)
        {
            SkillNodeUI child = nodes[i];
            if (child == null || child.nodeDefinition == null)
                continue;

            child.ClearParentConnectors();

            if (child.nodeDefinition.prerequisites == null)
                continue;

            foreach (var prereq in child.nodeDefinition.prerequisites)
            {
                if (prereq.node == null)
                    continue;

                if (!nodeLookup.TryGetValue(prereq.node, out SkillNodeUI parent))
                    continue;

                if (parent.nodeRect == null || child.nodeRect == null)
                    continue;

                TreeNodeConnector connector = Instantiate(connectorPrefab, connectorParent);
                connector.Bind(parent.nodeRect, child.nodeRect);
                child.AddParentConnector(connector);
                spawnedConnectors.Add(connector);
            }
        }
    }

    // Refreshes all node visuals.
    public void RefreshAll()
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i] != null)
                nodes[i].Refresh();

        if (hoveredNode != null && infoPanel != null)
            infoPanel.Refresh(hoveredNode.nodeDefinition, runtime);
    }

    // Clears existing connectors.
    private void ClearConnectors()
    {
        for (int i = 0; i < spawnedConnectors.Count; i++)
        {
            if (spawnedConnectors[i] != null)
                DestroyObjectSafe(spawnedConnectors[i].gameObject);
        }
        spawnedConnectors.Clear();

    #if UNITY_EDITOR
        if (!Application.isPlaying && connectorParent != null)
        {
            for (int i = connectorParent.childCount - 1; i >= 0; i--)
            {
                Transform child = connectorParent.GetChild(i);
                if (child != null && child.GetComponent<TreeNodeConnector>() != null)
                    DestroyObjectSafe(child.gameObject);
            }
        }
    #endif
    }


    // Shows hover info for a node.
    public void ShowInfo(SkillNodeUI node, bool altOffset = false)
    {
        hoveredNode = node;

        if (infoPanel == null || node == null)
            return;

        infoPanel.Show(node.nodeDefinition, runtime, altOffset);
        // FollowMousePosition(GetCursorPosition());
    }

    // Hides the hover info.
    public void HideInfo()
    {
        hoveredNode = null;

        if (infoPanel != null)
            infoPanel.Hide();
    }



    // Returns the current cursor position.
    private Vector3 GetCursorPosition()
    {
        if (WorldCursor.instance != null)
            return WorldCursor.instance.GetCursorPosition();

        return Input.mousePosition;
    }




    public void BuildFromLayout(bool editorPreview = false)
    {
        if (layoutDefinition == null || nodePrefab == null || nodeParent == null)
            return;

        ClearConnectors();

        // Clear old nodes
        for (int i = nodeParent.childCount - 1; i >= 0; i--)
            DestroyObjectSafe(nodeParent.GetChild(i).gameObject);

        nodes.Clear();

        // Spawn nodes
        foreach (var entry in layoutDefinition.entries)
        {
            if (entry.node == null)
                continue;

            SkillNodeUI node = Instantiate(nodePrefab, nodeParent);

            node.CacheDraggableOrigin();

            RectTransform rt = node.GetComponent<RectTransform>();
            rt.anchoredPosition = entry.anchoredPosition;

            node.nodeDefinition = entry.node;
            node.uiController = this;
            nodes.Add(node);

            if (editorPreview)
                ApplyEditorGhostNode(node);
        }

        BuildLookup();
        BuildConnectors();

        if (editorPreview)
            ApplyEditorGhostConnectors();
        else
            RefreshAll();
    }



    #if UNITY_EDITOR
    private void OnLayoutDefinitionChanged(SkillTreeLayoutDefinition changedLayout)
    {
        if (Application.isPlaying || !previewLayoutInEditor)
            return;

        if (changedLayout == layoutDefinition)
            QueueEditorPreviewRebuild();
    }
    #endif

    private void QueueEditorPreviewRebuild()
    {
    #if UNITY_EDITOR
        if (Application.isPlaying || !previewLayoutInEditor)
            return;

        if (editorPreviewRebuildQueued)
            return;

        editorPreviewRebuildQueued = true;
        UnityEditor.EditorApplication.delayCall += RebuildEditorPreviewIfNeeded;
    #endif
    }

    #if UNITY_EDITOR
    private void RebuildEditorPreviewIfNeeded()
    {
        editorPreviewRebuildQueued = false;

        if (this == null || Application.isPlaying || !previewLayoutInEditor)
            return;

        BuildFromLayout(true);
    }
    #endif

    private void ApplyEditorGhostNode(SkillNodeUI node)
    {
        if (node == null)
            return;

        CanvasGroup group = node.nodeCanvasGroup != null
            ? node.nodeCanvasGroup
            : node.GetComponent<CanvasGroup>();

        if (group == null)
            group = node.gameObject.AddComponent<CanvasGroup>();

        group.alpha = Mathf.Clamp01(editorNodeGhostAlpha);
        group.interactable = false;
        group.blocksRaycasts = false;

        if (node.draggable != null)
            node.draggable.enabled = false;

        Graphic[] graphics = node.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;
    }

    private void ApplyEditorGhostConnectors()
    {
        float alpha = Mathf.Clamp01(editorConnectorGhostAlpha);

        for (int i = 0; i < spawnedConnectors.Count; i++)
        {
            TreeNodeConnector connector = spawnedConnectors[i];
            if (connector == null)
                continue;

            CanvasGroup cg = connector.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = connector.gameObject.AddComponent<CanvasGroup>();

            cg.alpha = alpha;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    private void DestroyObjectSafe(Object target)
    {
        if (target == null)
            return;

    #if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(target);
        else
    #endif
            Destroy(target);
    }


}
