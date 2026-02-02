using System;
using UnityEngine;

/*
 * PlanetUpgradeInstance
 * ---------------------
 * Serializable, lightweight instance for a planet upgrade.
 * Stores the upgrade id and current level.
 */

[Serializable]
public class PlanetUpgradeInstance
{
    public PlanetUpgradeId id;
    public int level = 1;
}
