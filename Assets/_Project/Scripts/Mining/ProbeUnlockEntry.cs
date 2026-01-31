using System.Collections.Generic;
using UnityEngine;

// Stores the unlock state for each probe type in a fixed order.
[System.Serializable]
public class ProbeUnlockEntry
{
    public ProbeType type;
    public bool unlocked;
}
