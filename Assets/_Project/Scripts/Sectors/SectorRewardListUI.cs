using System.Collections.Generic;
using UnityEngine;

/*
 * SectorRewardListUI
 * ------------------
 * Builds a stacked list of reward panels for hover/reward UI.
 */
public class SectorRewardListUI : MonoBehaviour
{
    public Transform contentRoot;
    public SectorRewardEntryUI entryPrefab;

    // Clears all reward entries from the list.
    public void Clear()
    {
        if (contentRoot == null)
            return;

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);
    }

    // Populates the list with reward entries.
    public void SetRewards(List<SectorRewardEntry> rewards)
    {
        Clear();

        if (contentRoot == null || entryPrefab == null || rewards == null)
            return;

        for (int i = 0; i < rewards.Count; i++)
        {
            SectorRewardEntryUI entry = Instantiate(entryPrefab, contentRoot);
            entry.Bind(rewards[i]);
        }
    }
}
