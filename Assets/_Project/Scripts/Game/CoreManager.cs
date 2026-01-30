using UnityEngine;

public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance;

    public double totalCores;

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

    public void AddCores(double amount)
    {
        totalCores += amount;
    }

    public bool SpendCores(double amount)
    {
        if (totalCores < amount)
            return false;

        totalCores -= amount;
        return true;
    }
}
