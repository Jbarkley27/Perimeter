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

    public bool autoStartBattlePhase = true;

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
        if (autoStartBattlePhase) StartBattlePhase();
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
        Debug.Log("Run Ended....");

        // Pause Signal
        // SignalManager.Instance.PauseSignal();
        

        // Clear active enemies
        GlobalDataStore.Instance.EnemyPooler.ClearAllActiveEnemies();


        // Disable Player Controls
        GamePaused = true;


        // Open End Run Screen
        RunManager.Instance.StartShowEndRunScreen();
    }





    public IEnumerator RestartRun()
    {
        Debug.Log(RunAttempts == 0 ? "Starting Run" : "Restarting Run...");

        // Restor Barrier Signal
        GlobalDataStore.Instance.BarrierModule.ResetBarrier();


        // Sector Reset
        SectorManager.Instance.ResetSectors();

        // Reset Signal UI
        RunManager.Instance.ResetRun();

        // Reset Enemy Manager
        EnemyManager.Instance.Reset();

        // Reset Glass Manager
        GlassManager.Instance.ResetGlassThisRun();

        RunAttempts += 1;

        GamePaused = false;

        // Reset world cursor state
        WorldCursor.instance.ResetState();

        // Hide Skill Tree Screen
        RunManager.Instance.HideEndRunScreen();
        ConsoleUIManager.Instance.CloseConsole();
        RunManager.Instance.HideEndRunScreen();

        // wait a bit before restarting signal
        yield return new WaitForSeconds(startSignalDelay);

        // Reset Enemy Waves
        GlobalDataStore.Instance.WaveSpawner.Reset();

        // Reset all skill cooldowns
        GlobalDataStore.Instance.SkillCaster.ResetAllSkillCooldowns();

        // Start Signal Countdown
        // SignalManager.Instance.ResumeSignal();
    }


}
