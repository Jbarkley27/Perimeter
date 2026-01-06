using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public WaveSpawner waveSpawner;
    public int RunAttempts = 0;
    public bool GamePaused = false;
    public float signalSpawnDelay = 2.0f;
    public float startSignalDelay = 3f;


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
        StartBattlePhase();
        RunAttempts = -1;
    }



    // public IEnumerator PrepareBattlePhase()
    // {
    //     yield return null;
    // }


    public void StartBattlePhase()
    {
        StartCoroutine(RestartRun());
    }



    public void EndRun()
    {
        Debug.Log("Run Ended. Returning to Main Menu...");

        // Pause Signal
        SignalManager.Instance.PauseSignal();
        

        // Clear active enemies
        GlobalDataStore.Instance.EnemyPooler.ClearAllActiveEnemies();


        // Disable Player Controls
        GamePaused = true;


        // Open End Run Screen
        SkillTree.Instance.ShowEndRunScreen();
    }





    public IEnumerator RestartRun()
    {
        Debug.Log(RunAttempts == 0 ? "Starting Run" : "Restarting Run...");

        // Broadcast Signal
        StartCoroutine(SignalManager.Instance.BroadcastSignal());

        // Reset Signal UI
        SignalManager.Instance.ResetSignalUI();

        RunAttempts += 1;

        GamePaused = false;

        // Reset world cursor state
        WorldCursor.instance.ResetState();

        // Hide Skill Tree Screen
        SkillTree.Instance.HideEndRunScreen();
        SkillTree.Instance.CloseSkillTree();

        // wait a bit before restarting signal
        yield return new WaitForSeconds(startSignalDelay);

        // Reset Enemy Waves
        GlobalDataStore.Instance.WaveSpawner.Reset();

        // Reset all skill cooldowns
        GlobalDataStore.Instance.SkillCaster.ResetAllSkillCooldowns();

        // Start Signal Countdown
        SignalManager.Instance.ResumeSignal();
    }





    public void OpenMaintenanceMenu()
    {
        // Pause Signal
        SignalManager.Instance.PauseSignal();

        // Show Maintenance Menu UI
        SkillTree.Instance.OpenSkillTree();
    }
}
