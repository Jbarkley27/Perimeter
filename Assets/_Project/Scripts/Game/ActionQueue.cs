using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueue : MonoBehaviour
{
    public static ActionQueue Instance { get; private set; }
    private readonly Queue<GameAction> queue = new();
    private bool isRunning;
    public GameAction CurrentAction { get; private set; }


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



    public void Enqueue(GameAction action)
    {
        Debug.Log($"[ActionQueue] Enqueued: {action.DebugName}");
        queue.Enqueue(action);

        if (!isRunning)
            StartCoroutine(ProcessQueue());
    }

    

    private IEnumerator ProcessQueue()
    {
        isRunning = true;

        while (queue.Count > 0)
        {
            CurrentAction = queue.Dequeue();
            Debug.Log($"[ActionQueue] Dequeued: {CurrentAction.DebugName}");

            if (CurrentAction.IsValid != null && !CurrentAction.IsValid())
            {
                Debug.Log($"[ActionQueue] Skipped (Invalid): {CurrentAction.DebugName}");
                continue;
            }

            Debug.Log($"[ActionQueue] Executing: {CurrentAction.DebugName}");
            yield return StartCoroutine(CurrentAction.Execute());
            Debug.Log($"[ActionQueue] Completed: {CurrentAction.DebugName}");

            CurrentAction = null;
        }

        isRunning = false;
    }
}
