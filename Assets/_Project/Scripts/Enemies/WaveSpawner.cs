using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public Transform player;
    public float spawnRadius = 20f;


    [Header("Waves In Order")]
    public List<Wave> waves;
    private int currentWaveIndex = 0;
    private bool isSpawning = false;


    Coroutine waveRoutine;



    public IEnumerator StartNextWave()
    {
        Debug.Log("Starting Next Wave...");
        if (isSpawning || GameManager.Instance.GamePaused) yield return null;

        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
        }

        if (currentWaveIndex >= waves.Count) 
        {
            Debug.Log("All waves completed!");
            yield return null;
        }

        Wave wave = waves[currentWaveIndex];
        waveRoutine = StartCoroutine(ProcessWave(wave));
    }





    public IEnumerator ProcessWave(Wave wave)
    {
        isSpawning = true;
        Debug.Log($"Starting Wave: {wave.waveName}");

        foreach (var enemyID in wave.enemyIDs)
        {
            if (GameManager.Instance.GamePaused)
            {
                // stop spawning if the game is paused
                isSpawning = false;
                Debug.Log("Wave spawning paused.");
                yield break;
            }

            SpawnEnemy(enemyID);
            yield return new WaitForSeconds(wave.spawnDelay);
        }

        wave.isCompleted = true;
        waves[currentWaveIndex] = wave; // update stored struct state

        Debug.Log($"Wave Completed: {wave.waveName}");

        // Disabling for now to just use the same wave repeatedly
        currentWaveIndex++;
        isSpawning = false;
    }


    


    private void SpawnEnemy(EnemyDataStore.EnemyType enemyID)
    {
        if (GameManager.Instance.GamePaused)
        {
            Debug.Log("SpawnEnemy called while game is paused. Aborting spawn.");
            return;
        }


        GameObject enemy = EnemyPooler.Instance.GetEnemy(enemyID);

        float angle = Random.Range(0f, 360f);
        Vector3 pos = player.position + new Vector3(
            Mathf.Cos(angle) * spawnRadius,
            0,
            Mathf.Sin(angle) * spawnRadius
        );

        enemy.transform.position = pos;
    }


    public void Reset()
    {
        currentWaveIndex = 0;
        waves.ForEach(w => w.isCompleted = false);
        isSpawning = false;

        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
        }

        StartCoroutine(StartNextWave());
    }
}




[System.Serializable]
public struct Wave
{
    public string waveName;

    [Tooltip("Enemy IDs to spawn in order for this wave")]
    public List<EnemyDataStore.EnemyType> enemyIDs;

    [Tooltip("Delay between each enemy spawn in this wave")]
    public float spawnDelay;

    [HideInInspector] public bool isCompleted;

    public IEnumerator StartWaveRoutine(WaveSpawner spawner)
    {
        return spawner.ProcessWave(this);
    }
}
