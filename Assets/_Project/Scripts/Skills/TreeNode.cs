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
    public GameObject passiveLevelContainer; // parent of slider + background
    public CanvasGroup exclusiveInactiveIndicatorCanvasGroup;

    [Header("Tree Gating")]
    public TreeNode parentNode;
    public int requiredParentLevel = 1;

    [Header("State Visuals")]
    public GameObject lockedOverlay;
    public GameObject availableVisual;
    public GameObject exclusiveInactiveVisual;
    public Color passiveMaxLevelBorderColor = Color.yellow;

    [Header("Line Visuals")]
    public CanvasGroup lineCg = null;

    [Header("Unlock Animation")]
    public Transform unlockPunchTarget;
    public float unlockPunchScale = 0.15f;
    public float unlockPunchDuration = 0.25f;
    public int unlockPunchVibrato = 10;
    public float unlockPunchElasticity = 0.8f;

    private bool wasAvailable;

    [Header("Connector Spawning")]
    public RectTransform nodeRect;
    public TreeNodeConnector connectorPrefab;

    private TreeNodeConnector parentConnector;



    void Awake()
    {
        if (SkillTreeData.Instance != null)
            SkillTreeData.Instance.AddToAllNodes(this);

        if (SkillTreeData.Instance != null)
        SkillTreeData.Instance.AddToAllNodes(this);

        if (nodeRect == null)
            nodeRect = GetComponent<RectTransform>();

        if (connectorPrefab == null && SkillTreeUIManager.Instance != null)
            connectorPrefab = SkillTreeUIManager.Instance.nodeConnectorPrefab;
    }



    void Start()
    {
        // Turn off hover highlight at start
        // SkillTreeData.Instance.AddToAllNodes(this);
        // connectorPrefab = SkillTreeUIManager.Instance.nodeConnectorPrefab;
        if (hoverHighlightCanvasGroup) hoverHighlightCanvasGroup.gameObject.SetActive(false);
        if (exclusiveInactiveIndicatorCanvasGroup) exclusiveInactiveIndicatorCanvasGroup.gameObject.SetActive(false);
        
    }



    void Update()
    {
        bool isAvailable = IsAvailable();
        nodeState = isAvailable ? NodeState.Unlocked : NodeState.Locked;

        if (!wasAvailable && isAvailable)
            PlayUnlockPunch();
        
        wasAvailable = isAvailable;

        UpdateNodeUI();
        ManageDraggableState();
    }



    private void PlayUnlockPunch()
    {
        // var target = unlockPunchTarget != null ? unlockPunchTarget : transform;

        // target.DOKill();
        // target.DOPunchScale(Vector3.one * unlockPunchScale, unlockPunchDuration, unlockPunchVibrato, unlockPunchElasticity)
        //     .SetEase(Ease.OutCubic)
        //     .OnComplete(() => target.localScale = Vector3.one);
    }


    public void ManageDraggableState()
    {
        // Manage draggable state
        if (draggableComponent != null)
        {
            bool isActive = SkillTreeData.Instance.IsNodeActive(this);

            if (nodeState == NodeState.Locked || skillData == null || skillData.isPassive || !isActive)
                draggableComponent.enabled = false;
            else
                draggableComponent.enabled = true;

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
        if (skillData != null) SkillTreeUIManager.Instance.ShowSkillUIPanel(this);
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
        UpdateNodeUI();
    }




    // Initialize node and its children
    public void InitializeNode()
    {
        if (connectorPrefab == null && parentNode != null)
            connectorPrefab = parentNode.connectorPrefab;

        // Connect to children Nodes
        foreach (var child in children)
        {
            if (child == null) continue;

            if (child.nodeRect == null)
                child.nodeRect = child.GetComponent<RectTransform>();

            // Auto-assign parent if you want
            if (child.parentNode == null)
                child.parentNode = this;

            // Spawn connector from this -> child
            if (connectorPrefab != null && SkillTreeUIManager.Instance.connectorParent != null && nodeRect != null && child.nodeRect != null)
            {
                var connector = Instantiate(connectorPrefab, SkillTreeUIManager.Instance.connectorParent);
                connector.Bind(nodeRect, child.nodeRect);

                // let the child control its own parent connector alpha
                child.parentConnector = connector;
            }

            child.InitializeNode();
        }

        // Node state is now driven by availability
        nodeState = IsAvailable() ? NodeState.Unlocked : NodeState.Locked;

        if (skillData != null && nodeIcon != null)
            nodeIcon.sprite = skillData.icon;

        if (skillData != null && nodeBackground != null)
            nodeBackground.color = GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);

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

         wasAvailable = IsAvailable();
         UpdateNodeUI();
    }




    // Update the visual state of the node based on its current state
    public void UpdateNodeUI()
    {
        if (skillData == null)
            return;

        bool isAvailable = IsAvailable();
        bool isActive = SkillTreeData.Instance.IsNodeActive(this);

        bool isExclusive = !string.IsNullOrEmpty(skillData.exclusiveGroupId);

        if (parentConnector != null)
            lineCg = parentConnector.GetComponent<CanvasGroup>();

        if (lineCg)
        {
            if (!isAvailable)
                lineCg.alpha = 0.1f;
            else if (isActive)
                lineCg.alpha = 1f;
            else
                lineCg.alpha = 0.5f;
        }

        bool isExclusiveInactive = isExclusive && !skillData.isExclusiveActive;
        bool isPassiveMax = skillData.isPassive && skillData.currentLevel >= skillData.maxLevel;
        bool isPassive = skillData.isPassive;

        bool hasPurchased = isExclusive || isPassive
            ? skillData.currentLevel > 0
            : skillData.isUnlocked;

        // Locked overlay
        if (lockedOverlay)
            lockedOverlay.SetActive(!isAvailable);

        // Available visual (only when available AND not active)
        if (availableVisual)
            availableVisual.SetActive(isAvailable && !hasPurchased);

        // Exclusive inactive overlay
        if (exclusiveInactiveVisual)
            exclusiveInactiveVisual.SetActive(isExclusiveInactive);

        // Passive max level border color
        if (nodeBorderImageRoot)
            nodeBorderImageRoot.color = isPassiveMax ? passiveMaxLevelBorderColor : Color.white;

        // Passive level slider
        if (passiveLevelContainer != null)
        {
            if (skillData.isPassive && IsAvailable())
            {
                passiveLevelContainer.SetActive(true);
                if (passiveLevelSlider != null)
                    passiveLevelSlider.value = skillData.currentLevel;
            }
            else
            {
                passiveLevelContainer.SetActive(false);
            }
        }


        // Node background color by element for effects that change element type
        if (nodeBackground != null)
        {
            nodeBackground.color =
                GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);
        }

        // Affordability icon only when available AND not max level
        if (canAffordImage != null)
        {
            bool isMaxLevel = skillData.isPassive && skillData.currentLevel >= skillData.maxLevel;
            bool canAfford = GlassManager.Instance.CanAffordNodePurchase(skillData.cost);

            bool showAfford = false;

            if (IsAvailable() && canAfford && !isMaxLevel)
            {
                if (skillData.isPassive)
                {
                    // keep showing for passive upgrades until max
                    showAfford = true;
                }
                else
                {
                    // active/exclusive only show before first purchase
                    hasPurchased = !string.IsNullOrEmpty(skillData.exclusiveGroupId)
                        ? skillData.currentLevel > 0
                        : skillData.isUnlocked;

                    showAfford = !hasPurchased;
                }
            }

            canAffordImage.gameObject.SetActive(showAfford);
        }
    }





    public void PurchaseOrUpgradeNode()
    {
        if (skillData == null)
            return;

        if (!IsAvailable())
        {
            Debug.Log("Node locked by parent requirement.");
            return;
        }

        // Exclusive group nodes: pay only on first unlock, switching is free
        if (!string.IsNullOrEmpty(skillData.exclusiveGroupId))
        {
            bool isMaxLevel = skillData.currentLevel >= skillData.maxLevel;

            if (!isMaxLevel)
            {
                if (!GlassManager.Instance.SpendGlass(skillData.cost))
                {
                    Debug.Log("Not enough Glass to purchase this skill.");
                    return;
                }

                SetNodeState(NodeState.Unlocked);
                skillData.currentLevel = Mathf.Max(1, skillData.currentLevel);
            }

            SkillTreeData.Instance.SetExclusiveState(skillData);
            return;
        }

        // Active skill purchase
        if (!skillData.isPassive)
        {
            Debug.Log("Attempting to purchase active skill node.");
            if (skillData.isUnlocked)
                return;

            if (!GlassManager.Instance.SpendGlass(skillData.cost))
            {
                Debug.Log("Not enough Glass to purchase this skill.");
                return;
            }

            SetNodeState(NodeState.Unlocked);
            skillData.isUnlocked = true;
            SkillTreeData.Instance.RebuildAll();
            return;
        }

        // Passive skill upgrade
        if (!skillData.isUnlocked)
            SetNodeState(NodeState.Unlocked);

        if (skillData.currentLevel < skillData.maxLevel)
        {
            if (GlassManager.Instance.SpendGlass(skillData.cost))
            {
                IncreaseSkillLevel();
                SkillTreeData.Instance.RebuildAll();
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


    public bool IsAvailable()
    {
        if (parentNode == null || parentNode.skillData == null)
            return true;

        return GetNodeProgress(parentNode) >= requiredParentLevel;
    }

    private int GetNodeProgress(TreeNode node)
    {
        if (node == null || node.skillData == null)
            return 0;

        var data = node.skillData;

        if (!string.IsNullOrEmpty(data.exclusiveGroupId))
            return data.currentLevel; // exclusive nodes are 0/1

        if (data.isPassive)
            return data.currentLevel;

        return data.isUnlocked ? 1 : 0;
    }
}