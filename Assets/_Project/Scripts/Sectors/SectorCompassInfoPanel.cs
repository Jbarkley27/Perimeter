using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
 * SectorCompassInfoPanel
 * ----------------------
 * Hover panel for a compass option (modifier + rewards).
 */
public class SectorCompassInfoPanel : MonoBehaviour
{
    public TMP_Text directionText;
    public TMP_Text modifierNameText;
    public TMP_Text rarityText;
    public TMP_Text descriptionText;

    [Header("Rewards")]
    public SectorRewardListUI rewardsList;

    // Shows the info panel for a compass choice.
    public void Show(SectorCompassChoice choice, List<SectorRewardEntry> rewardEntries)
    {
        if (directionText != null)
            directionText.text = choice.direction.ToString();

        if (modifierNameText != null)
            modifierNameText.text = choice.modifier != null ? choice.modifier.displayName : "Hold Course";

        if (rarityText != null)
            rarityText.text = choice.modifier != null ? choice.modifier.rarity.ToString() : "Neutral";

        if (descriptionText != null)
            descriptionText.text = choice.modifier != null ? choice.modifier.description : "No modifier effects.";

        if (rewardsList != null)
            rewardsList.SetRewards(rewardEntries);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
