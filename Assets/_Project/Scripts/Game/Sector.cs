using UnityEngine;

// Scriptable object to hold sector data

[CreateAssetMenu(fileName = "New Sector", menuName = "Game/Sector")]
public class Sector : ScriptableObject
{
    public int sectorID;
    public string sectorName;
    
}