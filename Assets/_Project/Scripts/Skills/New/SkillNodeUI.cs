using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * SkillNodeUI
 * -----------
 * Single UI node that adapts its visuals based on SkillNodeDefinition + runtime state.
 */


public enum NodeSpriteRole
{
    Fill,      // solid background
    Border,    // hollow/outline
    Locked,    // locked overlay
    Highlight  // hover highlight
}

[System.Serializable]
public struct NodeSpriteTarget
{
    public NodeSpriteRole role;
    public Image image;
}



public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Definition")]
    public SkillNodeDefinition nodeDefinition;

    [Header("UI References")]
    public Image nodeIcon;
    public Image nodeBackground;
    public Image nodeBorder;
    public CanvasGroup nodeCanvasGroup;
    public CanvasGroup hoverHighlightCanvasGroup;
    public GameObject lockedVisualRoot;
    public GameObject availableVisualRoot;
    public GameObject exclusiveInactiveVisual;
    public Slider levelSlider;
    public GameObject levelSliderRoot;
    public SkillDraggable draggable;
    public GameObject canAffordImage;

    [Header("Connector")]
    public RectTransform nodeRect;

    [Header("Exclusive Visuals")]
    [Range(0f, 1f)] public float exclusiveInactiveAlpha = 0.35f;

    [Header("Hover Punch")]
    public Transform hoverPunchTarget;
    public float hoverPunchScale = 0.08f;
    public float hoverPunchDuration = 0.12f;
    public int hoverPunchVibrato = 8;
    public float hoverPunchElasticity = 1f;

    [Header("Owner")]
    public SkillTreeUIController uiController;

    private readonly List<TreeNodeConnector> parentConnectors = new List<TreeNodeConnector>();

    [System.Serializable]
    public struct NodeTypeSprites
    {
        public Sprite fill;
        public Sprite border;
        public Sprite locked;
        public Sprite highlight;
    }

    [Header("Type Sprites")]
    public NodeTypeSprites activeSprites;
    public NodeTypeSprites passiveSprites;
    public NodeTypeSprites exclusiveSprites;

    [Header("Sprite Targets")]
    public List<NodeSpriteTarget> spriteTargets = new List<NodeSpriteTarget>();

    // Visual/interaction state for a skill tree node.
    public enum SkillNodeVisualState
    {
        Hidden,            // prereqs not met
        Locked,            // visible but not yet purchased
        Unlocked,          // purchased/active
        ExclusiveInactive  // purchased but not the chosen exclusive
    }

    [Header("Debug")]
    public SkillNodeVisualState currentState;




    private void Awake()
    {
        if (nodeRect == null)
            nodeRect = GetComponent<RectTransform>();

        if (hoverPunchTarget == null)
            hoverPunchTarget = transform;

        if (hoverHighlightCanvasGroup != null)
            hoverHighlightCanvasGroup.gameObject.SetActive(false);

        if (levelSliderRoot == null && levelSlider != null)
            Debug.LogWarning($"[SkillNodeUI] levelSliderRoot not assigned on {name}. Slider will be toggled directly.");

        if (levelSliderRoot != null)
            levelSliderRoot.SetActive(false);

        Refresh();
    }


    private SkillNodeVisualState GetVisualState(SkillTreeRuntime runtime)
    {
        if (runtime == null || nodeDefinition == null)
            return SkillNodeVisualState.Hidden;

        if (!runtime.IsAvailable(nodeDefinition))
            return SkillNodeVisualState.Hidden;

        int level = runtime.GetLevel(nodeDefinition);
        if (level <= 0)
            return SkillNodeVisualState.Locked;

        if (nodeDefinition.IsExclusive && !runtime.IsExclusiveActive(nodeDefinition))
            return SkillNodeVisualState.ExclusiveInactive;

        return SkillNodeVisualState.Unlocked;
    }


    // Refresh the node visuals based on runtime state.
    public void Refresh()
    {
        SkillTreeRuntime runtime = uiController != null ? uiController.runtime : SkillTreeRuntime.Instance;
        if (nodeDefinition == null || runtime == null)
            return;

        ApplyTypeSprites();

        SkillNodeVisualState state = GetVisualState(runtime);

        currentState = state;


        if (state == SkillNodeVisualState.Hidden)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        int level = runtime.GetLevel(nodeDefinition);
        bool isUnlocked = level > 0;
        bool isExclusive = nodeDefinition.IsExclusive;
        bool isExclusiveActive = runtime.IsExclusiveActive(nodeDefinition);

        // Icon + element color.
        if (nodeIcon != null && nodeDefinition.skill != null)
            nodeIcon.sprite = nodeDefinition.skill.icon;

        if (nodeBackground != null)
        {
            nodeBackground.color =
                GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(nodeDefinition.element);
        }

        // Locked vs available visuals.
        if (lockedVisualRoot != null)
            lockedVisualRoot.SetActive(state == SkillNodeVisualState.Locked);

        if (availableVisualRoot != null)
            availableVisualRoot.SetActive(state == SkillNodeVisualState.Locked);

        // Exclusive inactive visuals / alpha.
        if (exclusiveInactiveVisual != null)
            exclusiveInactiveVisual.SetActive(state == SkillNodeVisualState.ExclusiveInactive);

        if (nodeCanvasGroup != null)
        {
            nodeCanvasGroup.alpha =
                (state == SkillNodeVisualState.ExclusiveInactive) ? exclusiveInactiveAlpha : 1f;
        }

        // Slider for passive nodes only, hidden if maxLevel == 1.
        bool showSlider = nodeDefinition.IsPassive && nodeDefinition.maxLevel > 1;

        if (levelSliderRoot != null)
            levelSliderRoot.SetActive(showSlider);
        else if (levelSlider != null)
            levelSlider.gameObject.SetActive(showSlider);

        if (levelSlider != null && nodeDefinition.IsPassive)
        {
            levelSlider.maxValue = nodeDefinition.maxLevel;
            levelSlider.value = level;
        }

        // Drag only for active skills that are unlocked.
        if (draggable != null)
        {
            bool allowDrag = nodeDefinition.nodeType == SkillNodeType.Active && currentState == SkillNodeVisualState.Unlocked && nodeDefinition.skill != null;

            Debug.Log($"[SkillNodeUI] Setting draggable for node {nodeDefinition.displayName}: allowDrag={allowDrag}");
            if (nodeDefinition.IsExclusive)
                allowDrag = false;

            draggable.enabled = allowDrag;
            draggable.nodeUI = this;
            draggable.skillData = allowDrag ? nodeDefinition.skill : null;
        }

        // Connector alpha based on availability / active.
        for (int i = 0; i < parentConnectors.Count; i++)
        {
            TreeNodeConnector connector = parentConnectors[i];
            if (connector == null) continue;

            CanvasGroup cg = connector.GetComponent<CanvasGroup>();
            if (cg == null) continue;

            if (state == SkillNodeVisualState.Hidden)
                cg.alpha = 0.1f;
            else if (isExclusive && isUnlocked && !isExclusiveActive)
                cg.alpha = 0.5f;
            else if (isUnlocked)
                cg.alpha = 1f;
            else
                cg.alpha = 0.5f;
        }

        // Can afford indicator for locked nodes.
        if (canAffordImage != null) 
        {
            if (draggable != null && draggable.currentSlot != null)
            {
                canAffordImage.SetActive(false);
                return;
            }

            bool canAfford = state != SkillNodeVisualState.Hidden 
                    && SkillTreeRuntime.Instance != null 
                    && SkillTreeRuntime.Instance.GetCostForLevel(nodeDefinition, level + 1) <= (GlassManager.Instance != null ? GlassManager.Instance.GetTotalGlassShardsCollected() : -1)
                    && SkillTreeRuntime.Instance.IsAtMaxLevel(nodeDefinition) == false;

            canAffordImage.SetActive(canAfford);
        }
    }



    private NodeTypeSprites GetSpritesForType()
    {
        switch (nodeDefinition.nodeType)
        {
            case SkillNodeType.Active: return activeSprites;
            case SkillNodeType.Passive: return passiveSprites;
            case SkillNodeType.Exclusive: return exclusiveSprites;
            default: return activeSprites;
        }
    }

    private void ApplyTypeSprites()
    {
        NodeTypeSprites sprites = GetSpritesForType();

        for (int i = 0; i < spriteTargets.Count; i++)
        {
            var target = spriteTargets[i];
            if (target.image == null) continue;

            switch (target.role)
            {
                case NodeSpriteRole.Fill: target.image.sprite = sprites.fill; break;
                case NodeSpriteRole.Border: target.image.sprite = sprites.border; break;
                case NodeSpriteRole.Locked: target.image.sprite = sprites.locked; break;
                case NodeSpriteRole.Highlight: target.image.sprite = sprites.highlight; break;
            }
        }
    }


    // Adds a parent connector (used by the UI controller).
    public void AddParentConnector(TreeNodeConnector connector)
    {
        if (connector == null)
            return;

        parentConnectors.Add(connector);
    }

    // Clears all parent connectors.
    public void ClearParentConnectors()
    {
        parentConnectors.Clear();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (draggable != null && draggable.Snapping)
        {
            Debug.Log("Pointer entered while draggable is snapping. Ignoring hover.");
            return;
        }

        Debug.Log($"Pointer entered node {nodeDefinition?.displayName}");

        if (hoverHighlightCanvasGroup != null)
            hoverHighlightCanvasGroup.gameObject.SetActive(true);

        PlayHoverPunch();

        SkillTreeUIController controller = uiController != null
            ? uiController
            : GlobalDataStore.Instance != null ? GlobalDataStore.Instance.SkillTreeUIController : null;

        if (controller != null)
        {
            bool altOffset = false;
            if (draggable == null)
            {
                altOffset = false;
            }
            else
            {
                altOffset = draggable.currentSlot != null;
                altOffset = SkillLoadout.Instance.IsSkillEquipped(nodeDefinition.skill);
            }

            controller.ShowInfo(this, altOffset);
        }
        else
        {
            Debug.LogWarning($"[SkillNodeUI] No SkillTreeUIController found to show info for node {nodeDefinition?.displayName}");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverHighlightCanvasGroup != null)
            hoverHighlightCanvasGroup.gameObject.SetActive(false);

        SkillTreeUIController controller = uiController != null
            ? uiController
            : GlobalDataStore.Instance != null ? GlobalDataStore.Instance.SkillTreeUIController : null;

        if (controller != null)
            controller.HideInfo();
    }

    public void ForcePointerExit()
    {
        OnPointerExit(null);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (nodeDefinition == null || SkillTreeRuntime.Instance == null)
            return;

        bool changed = SkillTreeRuntime.Instance.TryPurchaseOrUpgrade(nodeDefinition);
        if (!changed)
            return;

        // Always refresh this node so visuals update immediately.
        Refresh();

        SkillTreeUIController controller = uiController != null
            ? uiController
            : GlobalDataStore.Instance != null ? GlobalDataStore.Instance.SkillTreeUIController : null;

        if (controller != null)
            controller.RefreshAll();
    }




    // Called after spawn to lock the drag return target.
    public void CacheDraggableOrigin()
    {
        if (draggable != null)
            draggable.CacheOriginalTransform();
    }


    // Small punch animation when hovered.
    private void PlayHoverPunch()
    {
        if (hoverPunchTarget == null)
            return;

        hoverPunchTarget.DOKill();
        hoverPunchTarget.DOPunchScale(Vector3.one * hoverPunchScale, hoverPunchDuration, hoverPunchVibrato, hoverPunchElasticity)
            .OnComplete(() => hoverPunchTarget.localScale = Vector3.one);
    }
}
