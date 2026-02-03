using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * SectorCompassNodeUI
 * -------------------
 * UI button for a single compass direction.
 */
public class SectorCompassNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SectorDirection direction;
    public Button button;

    private SectorCompassChoice boundChoice;
    private bool hasChoice;
    private SectorCompassUIController owner;

    public void Bind(SectorCompassChoice choice, SectorCompassUIController ownerController)
    {
        boundChoice = choice;
        hasChoice = true;
        owner = ownerController;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.RemoveAllListeners();

        if (button != null)
            button.onClick.AddListener(OnClicked);

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        hasChoice = false;
        owner = null;

        if (button != null)
            button.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hasChoice && owner != null)
            owner.ShowInfo(boundChoice);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (owner != null)
            owner.HideInfo();
    }

    private void OnClicked()
    {
        if (hasChoice && owner != null)
            owner.SelectChoice(boundChoice);
    }
}
