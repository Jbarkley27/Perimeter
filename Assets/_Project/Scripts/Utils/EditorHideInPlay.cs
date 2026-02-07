using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EditorHideInPlayTarget : MonoBehaviour
{
    public GameObject target;
    public List<GameObject> targets = new List<GameObject>();
    public bool hideInEditMode = true;

    private void OnEnable()
    {
        ApplyTargets();
    }

    private void ApplyTargets()
    {
        bool isPlaying = Application.isPlaying;
        bool shouldBeActive = isPlaying || !hideInEditMode;

        if (target != null)
            target.SetActive(shouldBeActive);

        for (int i = 0; i < targets.Count; i++)
        {
            GameObject obj = targets[i];
            if (obj != null)
                obj.SetActive(shouldBeActive);
        }
    }
}
