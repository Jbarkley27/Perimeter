using System.Collections.Generic;
using UnityEngine;

// Scriptable object to hold sector data
[CreateAssetMenu(fileName = "New Sector", menuName = "Game/Sector")]
public class Sector : ScriptableObject
{
    public int sectorID;
    public string sectorName;

    // Background color used when entering this sector.
    public Color backgroundColor = Color.black;

    // Base rewards granted for clearing this sector (non-modifier).
    public List<SectorRewardEntry> baseRewards = new List<SectorRewardEntry>();
}
