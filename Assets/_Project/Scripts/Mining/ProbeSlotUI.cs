using UnityEngine;
using UnityEngine.UI;

// Controls a slot visual including its probe icon.
public class ProbeSlotUI : MonoBehaviour
{
    public GameObject occupiedVisual;
    public GameObject emptyVisual;
    public Image iconImage;

    // Sets occupied state and icon.
    public void SetOccupied(bool isOccupied, Sprite icon = null)
    {
        if (occupiedVisual) occupiedVisual.SetActive(isOccupied);
        if (emptyVisual) emptyVisual.SetActive(!isOccupied);

        if (iconImage)
        {
            iconImage.enabled = isOccupied && icon != null;
            iconImage.sprite = icon;
        }
    }
}
