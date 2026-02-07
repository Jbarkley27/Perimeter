using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * WaveSpawner
 * -----------
 * Spawns enemies in wave order and handles optional bonus spawns
 * from active sector modifiers.
 */


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


    [Header("Wave Runtime State")]
    private int currentWaveTotalCount = 0;
    private int currentWaveExtraCount = 0;
    private Dictionary<int, int> bonusSpawnCounts = new Dictionary<int, int>();




    // Initializes wave counts (base list size).
    private void InitializeWaveCounts(Wave wave)
    {
        currentWaveExtraCount = 0;
        currentWaveTotalCount = wave.enemyIDs.Count;
        bonusSpawnCounts.Clear();


        if (EnemyManager.Instance != null)
            EnemyManager.Instance.SetWaveTargetCount(currentWaveTotalCount);
    }



    public IEnumerator StartNextWave()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogWarning("[WaveSpawner] No waves configured.");
            yield break;
        }

        if (isSpawning || GameManager.Instance.GamePaused) yield return null;

        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
        }

        if (currentWaveIndex >= waves.Count) 
        {
            yield return null;
        }

        Wave wave = waves[currentWaveIndex];
        InitializeWaveCounts(wave);
        waveRoutine = StartCoroutine(ProcessWave(wave));
    }





    public IEnumerator ProcessWave(Wave wave)
    {
        isSpawning = true;

        foreach (var enemyID in wave.enemyIDs)
        {
            if (GameManager.Instance.GamePaused)
            {
                // stop spawning if the game is paused
                isSpawning = false;
                yield break;
            }

            SpawnEnemy(enemyID, true);
            yield return new WaitForSeconds(wave.spawnDelay);
        }

        wave.isCompleted = true;
        waves[currentWaveIndex] = wave; // update stored struct state


        // Disabling for now to just use the same wave repeatedly
        currentWaveIndex++;
        isSpawning = false;
    }


     public int GetCurrentCountOfEnemiesInWave()
    {
        if (currentWaveIndex >= waves.Count) return 0;
        if (currentWaveTotalCount <= 0)
            return waves[currentWaveIndex].enemyIDs.Count;

        return currentWaveTotalCount;
    }



    // Spawns a single enemy. If allowBonus is true, extra spawns may occur.
    private void SpawnEnemy(EnemyDataStore.EnemyType enemyID, bool allowBonus)
    {

        if (player == null)
        {
            Debug.LogWarning("[WaveSpawner] Player transform not assigned.");
            return;
        }


        if (GameManager.Instance.GamePaused)
        {
            return;
        }

        GameObject enemy = EnemyPooler.Instance.GetEnemy(enemyID);

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 pos = player.position + new Vector3(
            Mathf.Cos(angle) * spawnRadius,
            0,
            Mathf.Sin(angle) * spawnRadius
        );


        enemy.transform.position = pos;

        if (allowBonus)
            TrySpawnBonusEnemies();
    }


    // Rolls and spawns extra enemies based on active sector modifiers.
        private void TrySpawnBonusEnemies()
    {
        if (SectorManager.Instance == null)
            return;

        List<SectorEnemySpawnBonus> bonuses = SectorManager.Instance.GetActiveSpawnBonuses();
        if (bonuses == null || bonuses.Count == 0)
            return;

        int extraSpawned = 0;

        for (int i = 0; i < bonuses.Count; i++)
        {
            SectorEnemySpawnBonus bonus = bonuses[i];

            if (bonus.extraCount <= 0 || bonus.chance <= 0f)
                continue;

            int spawnedSoFar = 0;
            bonusSpawnCounts.TryGetValue(i, out spawnedSoFar);

            int remaining = bonus.maxExtraPerWave <= 0
                ? bonus.extraCount
                : Mathf.Max(0, bonus.maxExtraPerWave - spawnedSoFar);

            if (remaining <= 0)
                continue;

            if (Random.value <= bonus.chance)
            {
                int spawnCount = bonus.extraCount;
                if (bonus.maxExtraPerWave > 0)
                    spawnCount = Mathf.Min(spawnCount, remaining);

                for (int j = 0; j < spawnCount; j++)
                {
                    SpawnEnemy(bonus.enemyType, false);
                    extraSpawned++;
                }

                if (spawnCount > 0)
                    bonusSpawnCounts[i] = spawnedSoFar + spawnCount;
            }
        }

        if (extraSpawned > 0)
            RegisterExtraSpawns(extraSpawned);
    }


    // Tracks extra spawns so wave completion includes them.
    private void RegisterExtraSpawns(int count)
    {
        currentWaveExtraCount += count;
        currentWaveTotalCount += count;

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.AddWaveTargetCount(count);
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

        if (currentWaveIndex < waves.Count)
            InitializeWaveCounts(waves[currentWaveIndex]);


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
