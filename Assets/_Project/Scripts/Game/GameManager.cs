using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


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
        Debug.Log("Battle Phase Ready!");
    }


}
