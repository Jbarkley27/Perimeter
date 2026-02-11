using UnityEngine;

public class TreeNodeConnector : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform a;
    public RectTransform b;

    void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    public void Bind(RectTransform start, RectTransform end)
    {
        a = start;
        b = end;
        Refresh();
    }

    void LateUpdate()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (a == null || b == null || rectTransform == null)
            return;

        Vector3 mid = (a.position + b.position) * 0.5f;
        rectTransform.position = mid;
    }
}
