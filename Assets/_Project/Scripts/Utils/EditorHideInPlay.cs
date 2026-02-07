using UnityEngine;

[ExecuteAlways]
public class EditorHideInPlayTarget : MonoBehaviour
{
    public GameObject target;
    public bool hideInEditMode = true;

    private void OnEnable()
    {
        if (target == null) return;

        if (Application.isPlaying)
            target.SetActive(true);
        else if (hideInEditMode)
            target.SetActive(false);
    }
}
