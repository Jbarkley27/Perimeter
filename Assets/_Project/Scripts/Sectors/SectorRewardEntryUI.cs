using TMPro;
using UnityEngine;

/*
 * SectorRewardEntryUI
 * -------------------
 * UI row for a single reward entry (name + description).
 */
public class SectorRewardEntryUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    // Binds UI text from a reward entry.
    public void Bind(SectorRewardEntry entry)
    {
        if (nameText != null)
            nameText.text = entry.rewardName;

        if (descriptionText != null)
            descriptionText.text = entry.rewardDescription;
    }
}
