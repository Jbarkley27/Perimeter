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
    public bool RoundOver = false;

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
        RoundOver = false;
        if (autoStartBattlePhase) StartBattlePhase(true);
        RunAttempts = -1;
    }

    // resetSectors = true resets sector progression, pending choices, and active modifiers back to sector 1.
    /*
     * ignoreCompass = true skips showing the compass UI even if a choice is pending and starts the battle phase immediately.

     * For the console/mining “start run” button, you want resetSectors = false and ignoreCompass = false so it shows the compass if a choice is pending.
     */
    public void StartBattlePhaseIgnoreCompass()
    {
        StartBattlePhase(resetSectors: false, ignoreCompass: false);
    }




    public void StartBattlePhase(bool resetSectors = false, bool ignoreCompass = false)
    {
        if (!ignoreCompass && RunManager.Instance != null && RunManager.Instance.TryShowCompassOnly())
            return;

        StartCoroutine(RestartRun(resetSectors));
    }




    public void EndRun()
    {
        // Just in case multiple run ender calls happen
        if (RoundOver) return;
        RoundOver = true;

        



        // Clear active enemies
        GlobalDataStore.Instance.EnemyPooler.ClearAllActiveEnemies();


        // Disable Player Controls
        GamePaused = true;


        // Open End Run Screen
        RunManager.Instance.StartShowEndRunScreen();
    }





    public IEnumerator RestartRun(bool resetSectors)
    {
        RoundOver = false;

        // Restor Barrier Signal
        GlobalDataStore.Instance.BarrierModule.ResetHealthBarrier();

        // Sector Reset
        if (resetSectors)
            SectorManager.Instance.ResetSectors();


        // Reset Signal UI
        RunManager.Instance.ResetRun();

        // Reset Enemy Manager
        EnemyManager.Instance.Reset();

        // Reset Glass Manager
        GlassManager.Instance.ResetGlassThisRun();

        SkillTreeData.Instance.ResetTree();

        CompleteRun();

        // Reset world cursor state
        WorldCursor.instance.ResetState();

        // Hide Skill Tree Screen
        RunManager.Instance.HideEndRunScreen();
        ConsoleUIManager.Instance.CloseConsole();
        RunManager.Instance.HideEndRunScreen();

        RunManager.Instance.HideCompass();


        // wait a bit before restarting signal
        yield return new WaitForSeconds(startSignalDelay);


        GamePaused = false;

        // Reset Enemy Waves
        GlobalDataStore.Instance.WaveSpawner.Reset();

        // Reset all skill cooldowns
        GlobalDataStore.Instance.SkillCaster.ResetAllSkillCooldowns();
    }

    public void CompleteRun()
    {
        RunAttempts += 1;
    }
}
