using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public WaveSpawner waveSpawner;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    void Start()
    {
        ActionQueue.Instance.Enqueue(
            new GameAction(
                debugName: "Prepare Battle Phase",
                isValid: () => true,
                execute: () => PrepareBattlePhase()
            )
        );
    }



    public IEnumerator PrepareBattlePhase()
    {
        Debug.Log("Preparing Battle Phase...");
        yield return new WaitForSeconds(2f); // Simulate some preparation time
        StartBattlePhase();
        Debug.Log("Battle Phase Ready!");
    }


    public void StartBattlePhase()
    {
        ActionQueue.Instance.Enqueue(
            new GameAction(
                debugName: "Start Battle Phase",
                isValid: () => true,
                execute: () => waveSpawner.StartNextWave()
            )
        );
    }

}
