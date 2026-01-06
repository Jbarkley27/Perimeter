using System.Collections.Generic;
using UnityEngine;

public class EnemyPooler : MonoBehaviour
{
    public static EnemyPooler Instance;

    public enum EnemyType
    {
        BASIC,
        BOMBER,
        SNIPER,
        TANK,
        HEALER
    }

    [System.Serializable]
    public class EnemyPoolData
    {
        public EnemyType enemyID; 
        public GameObject prefab;
        public int initialAmount = 10;
    }

    [Header("Enemy Types to Pool")]
    public List<EnemyPoolData> enemyTypes = new List<EnemyPoolData>();


    [Header("Runtime Pools")]
    public Transform inactiveParent;
    public Transform activeParent;

    
    private Dictionary<EnemyType, Queue<GameObject>> pools = new Dictionary<EnemyType, Queue<GameObject>>();

    public List<EnemyAI> ActiveEnemies = new List<EnemyAI>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePools();
    }



    private void InitializePools()
    {
        ActiveEnemies.Clear();
        foreach (var type in enemyTypes)
        {
            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < type.initialAmount; i++)
            {
                GameObject obj = Instantiate(type.prefab, inactiveParent);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            pools.Add(type.enemyID, queue);
        }
    }



    public GameObject GetEnemy(EnemyType enemyID)
    {
        if (!pools.ContainsKey(enemyID))
        {
            Debug.LogError($"EnemyPooler: No pool found for ID '{enemyID}'");
            return null;
        }

        // Expand automatically if empty
        if (pools[enemyID].Count == 0)
            ExpandPool(enemyID);

        GameObject enemy = pools[enemyID].Dequeue();
        enemy.transform.SetParent(activeParent);
        enemy.SetActive(true);
        ActiveEnemies.Add(enemy.GetComponent<EnemyAI>());
        return enemy;
    }



    public void ReturnEnemyToPool(GameObject enemy, EnemyType enemyID)
    {
        if (!pools.ContainsKey(enemyID))
        {
            Debug.LogError($"EnemyPooler: Trying to return enemy to non-existent pool '{enemyID}'");
            Destroy(enemy);
            return;
        }
        // EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        // if (enemyAI != null)
        // {
        //     enemyAI.ResetEnemy();
        // }


        enemy.SetActive(false);
        enemy.transform.SetParent(inactiveParent);
        pools[enemyID].Enqueue(enemy);
        ActiveEnemies.Remove(enemy.GetComponent<EnemyAI>());
    }

    

    private void ExpandPool(EnemyType enemyID)
    {
        var data = enemyTypes.Find(t => t.enemyID == enemyID);
        if (data == null) return;

        GameObject obj = Instantiate(data.prefab, inactiveParent);
        obj.SetActive(false);
        pools[enemyID].Enqueue(obj);
    }


    public void ClearAllActiveEnemies()
    {
        Debug.Log("Clearing all active enemies...");
        foreach (var enemy in new List<EnemyAI>(ActiveEnemies))
        {
            if (enemy != null)
            {
                ReturnEnemyToPool(enemy.gameObject, enemy.enemyType);
            }
        }
    }
}
