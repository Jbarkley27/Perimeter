using UnityEngine;

/*
 * EditorHideInPlay
 * ----------------
 * Hides a GameObject in Edit Mode, but auto‑enables it when Play starts.
 */
[ExecuteAlways]
public class EditorHideInPlay : MonoBehaviour
{
    public bool hideInEditMode = true;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }
        else if (hideInEditMode)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}
