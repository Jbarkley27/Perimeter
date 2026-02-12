using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public CanvasGroup inventoryRoot;
    public List<CanvasGroup> inventorySections = new List<CanvasGroup>();
    public List<CanvasGroup> HUDCanvasGroup = new List<CanvasGroup>();
    public bool InventoryOpen = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;

        CloseInventory(); // Start with inventory closed
    }

    public void ToggleInventory()
    {
        if (InventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
            foreach (var hud in HUDCanvasGroup)
            {
                hud.DOKill();
                hud.DOFade(0, 0.3f);
            }
        }
    }


    public void OpenInventory()
    {
        InventoryOpen = true;
        inventoryRoot.DOKill(); // Kill any ongoing tweens to prevent conflicts
        inventoryRoot.gameObject.SetActive(true);
        inventoryRoot.alpha = 0;

        foreach (var section in inventorySections)
        {
            section.DOKill();
            section.alpha = 0;
            section.DOFade(1, 0.15f).SetDelay(.15f); // Start all sections at 0 alpha
        }

        inventoryRoot.DOFade(1, 0.3f);
    }

    public void CloseInventory()
    {
        inventoryRoot.DOKill(); // Kill any ongoing tweens to prevent conflicts

        foreach (var section in inventorySections)
        {
            section.DOKill();
            section.DOFade(0, 0.15f);
        }

        foreach (var hud in HUDCanvasGroup)
        {
            hud.DOKill();
            hud.DOFade(1, 0.3f);
        }

        inventoryRoot.DOFade(0, 0.3f).OnComplete(() =>
        {
            inventoryRoot.gameObject.SetActive(false);
        });
        
        InventoryOpen = false;
    }
}
