using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/*
 * ClickRaycastDebugger
 * --------------------
 * Logs all UI + world objects under the cursor on click.
 */
public class ClickRaycastDebugger : MonoBehaviour
{
    public bool logUI = true;
    public bool logWorld = true;
    public LayerMask worldMask = ~0;

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (logUI && EventSystem.current != null)
        {
            PointerEventData ped = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            Debug.Log($"[UI] Hits: {results.Count}");
            foreach (var r in results)
                Debug.Log($"[UI] {r.gameObject.name} ({r.module})");
        }

        if (logWorld && Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, worldMask))
                Debug.Log($"[World] {hit.collider.gameObject.name}");
            else
                Debug.Log("[World] No hit");
        }
    }
}
