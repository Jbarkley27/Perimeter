using System.Collections.Generic;
using UnityEngine;

/*
 * SkillTreeUIController
 * ---------------------
 * Manages UI nodes, hover panel, and auto‑generated connectors.
 */
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


    private void Awake()
    {
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
                Destroy(spawnedConnectors[i].gameObject);
        }
        spawnedConnectors.Clear();
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

    // Converts cursor to local canvas space and positions the panel.
    // private void FollowMousePosition(Vector3 mousePosition)
    // {
    //     if (infoPanelRect == null)
    //         return;

    //     RectTransform parentRect = infoPanelRect.parent as RectTransform;
    //     if (parentRect == null)
    //         return;

    //     Camera cam = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
    //         ? rootCanvas.worldCamera
    //         : null;

    //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //         parentRect,
    //         mousePosition,
    //         cam,
    //         out Vector2 localPoint
    //     );

    //     // infoPanelRect.anchoredPosition = localPoint + (Vector2)mouseOffset;
    // }

    // Returns the current cursor position.
    private Vector3 GetCursorPosition()
    {
        if (WorldCursor.instance != null)
            return WorldCursor.instance.GetCursorPosition();

        return Input.mousePosition;
    }




    public void BuildFromLayout()
    {
        if (layoutDefinition == null || nodePrefab == null || nodeParent == null)
            return;

        // Clear old nodes
        for (int i = nodeParent.childCount - 1; i >= 0; i--)
            Destroy(nodeParent.GetChild(i).gameObject);

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
        }

        BuildLookup();
        BuildConnectors();
        RefreshAll();
    }

}
