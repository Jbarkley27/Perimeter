using UnityEngine;
using System.Collections;

// This class is a library for enemy attack behaviors
// TODO: add the damage source when we create the Economy System so that enemy damage scales with the game

public class EnemyAttackLibrary : MonoBehaviour
{
    public enum EnemyAttackID
    {
        SINGLE_SHOT = 0,
        SPREAD_SHOT = 1
    }

    public GameObject enemyProjectilePrefab;

    public static EnemyAttackLibrary Instance { get; private set; }

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

    public IEnumerator PerformAttack(EnemyAttackID attackID, EnemyAI enemyContext, Transform[] projectileSources)
    {
        switch (attackID)
        {
            case EnemyAttackID.SINGLE_SHOT:
                if (projectileSources.Length < 1)
                {
                    Debug.LogError("SINGLE_SHOT attack requires at least 1 projectile source.");
                    yield break;
                }
                yield return SingleShotAttack(enemyContext, projectileSources[0]);
                break;

            case EnemyAttackID.SPREAD_SHOT:
                if (projectileSources.Length < 1)
                {
                    Debug.LogError("SPREAD_SHOT attack requires at least 2 projectile sources.");
                    yield break;
                }
                yield return SpreadShotAttack(enemyContext, projectileSources);
                break;

            default:
                Debug.LogError($"Unknown attack ID: {attackID}");
                yield break;
        }
    }



    public IEnumerator SingleShotAttack(EnemyAI enemyContext, Transform projectileSource)
    {
        // spawn an EnemyProjectile from the projectileSource
        EnemyProjectile projectile = Instantiate(
            enemyProjectilePrefab,
            projectileSource.position,
            projectileSource.rotation
        ).GetComponent<EnemyProjectile>();


        projectile.Launch(GlobalDataStore.Instance.PlayerPosition, 4, 100f);

        yield return new WaitForSeconds(0.1f);


    }

    public static IEnumerator SpreadShotAttack(EnemyAI enemyContext, Transform[] projectileSources)
    {
        // Example spread shot attack logic
        Debug.Log($"{enemyContext.gameObject.name} is performing Spread Shot Attack!");

        // Simulate attack delay
        yield return new WaitForSeconds(0.5f);

        // Here you would instantiate multiple projectiles from each source in a spread pattern

        yield return null;
    }
}


// public static class EnemyAttacks
//     {
//         public static IEnumerator SingleShotAttack(EnemyAI enemyContext, Transform projectileSource)
//         {
//             // spawn an EnemyProjectile from the projectileSource
//             EnemyProjectile projectile = Instantiate(
//                 GlobalDataStore.Instance.EnemyProjectilePrefab,
//                 projectileSource.position,
//                 projectileSource.rotation
//             ).GetComponent<EnemyProjectile>();


//         }

//         public static IEnumerator SpreadShotAttack(EnemyAI enemyContext, Transform[] projectileSources)
//         {
//             // Example spread shot attack logic
//             Debug.Log($"{enemyContext.gameObject.name} is performing Spread Shot Attack!");

//             // Simulate attack delay
//             yield return new WaitForSeconds(0.5f);

//             // Here you would instantiate multiple projectiles from each source in a spread pattern

//             yield return null;
//         }
//     }
