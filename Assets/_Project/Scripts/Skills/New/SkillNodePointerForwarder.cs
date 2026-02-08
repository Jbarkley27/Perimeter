using UnityEngine;
using UnityEngine.EventSystems;

public class SkillNodePointerForwarder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillNodeUI target;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (target != null)
            target.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (target != null)
            target.OnPointerExit(eventData);
    }
}
