using System.Collections.Generic;
using UnityEngine;

public class SectorManager : MonoBehaviour
{
    public static SectorManager Instance { get; private set; }
    public List<Sector> Sectors = new List<Sector>();
    public int currentSectorIndex = 0;
    public int maxSectors = 100;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    public void AdvanceToNextSector()
    {
        if (currentSectorIndex < maxSectors - 1)
        {
            currentSectorIndex++;
            Debug.Log("Advanced to Sector: " + currentSectorIndex);
        }
        else
        {
            Debug.Log("Already at maximum sector.");
        }
    }



    public void ResetSectors()
    {
        currentSectorIndex = 0;
        Debug.Log("Sectors reset to Sector: " + currentSectorIndex);
    }



    public bool IsAtMaxSector()
    {
        return currentSectorIndex >= maxSectors - 1;
    }

    public int GetCurrentSectorIndex()
    {
        return currentSectorIndex + 1;
    }

    public int GetNextSectorIndex()
    {
        return IsAtMaxSector() ? currentSectorIndex : currentSectorIndex + 2;
    }
}