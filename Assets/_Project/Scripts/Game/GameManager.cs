using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int RunAttempts = 0;
    public bool GamePaused = false;
    public bool autoStartBattlePhase = true;
    public bool RoundOver = false;
    public GameObject startScreenRoot;
    public CanvasGroup startScreenCanvasGroup;
    public bool SkipStartScreen = false;
    public bool SkipStartStory = false;
    public List<GameObject> objectsToDisableOnStartScreen = new List<GameObject>();
    public CanvasGroup GlobalUIRootCanvasGroup;

    public CanvasGroup tutorialRoot;
    public bool SkipTutorial = false;

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
        RunAttempts = -1;

        // Entry point for the game. Show the start screen, which will then lead into the story sequence and eventually the battle phase.
        ShowStartScreen();
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
        {
            if (ConsoleUIManager.Instance != null)
                ConsoleUIManager.Instance.CloseConsole(); 
            return;
        }

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

        GlobalUIRootCanvasGroup.alpha = 1;

        // Restor Barrier Signal
        GlobalDataStore.Instance.BarrierModule.ResetHealthBarrier();

        // Sector Reset
        if (resetSectors)
            SectorManager.Instance.ResetSectors();

        yield return new WaitForSeconds(0.1f);

        // Reset Signal UI
        RunManager.Instance.ResetRun();

        // Reset Enemy Manager
        EnemyManager.Instance.Reset();

        // Reset Glass Manager
        GlassManager.Instance.ResetGlassThisRun();

        // SkillTreeData.Instance.ResetTree();

        // CompleteRun();

        // Reset world cursor state
        WorldCursor.instance.ResetState();

        // Hide Skill Tree Screen
        RunManager.Instance.HideEndRunScreen();
        ConsoleUIManager.Instance.CloseConsole();

        RunManager.Instance.HideCompass();

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





    public void ShowStartScreen()
    {
        RunManager.Instance.ResetRun();
        // If skipping start screen, also check if we should skip the start story sequence. If so, skip everything and go straight to battle phase. If not, just skip the start screen and go to the story sequence.
        if (SkipStartScreen)
        {
            // Check if we should also skip the start story sequence
            if (SkipStartStory)
            {
                // Just skip everything and start the battle phase immediately
                Debug.Log("Skipping start screen and story, starting battle phase immediately.");

                // Turn off start screen UI
                if (startScreenRoot != null)
                    startScreenRoot.SetActive(false);

                
                StartCoroutine(ReadyGameStart(0));
            }
            else
            {
                Debug.Log("Skipping start screen, starting story sequence immediately.");
                StartCoroutine(ReadyGameStart(0));
            }

            // GlobalUIRootCanvasGroup.alpha = 1;
            return;
        }

        Debug.Log("Showing Start Screen");

        // Disable specified objects while start screen is active
        foreach (var obj in objectsToDisableOnStartScreen)
        {   if (obj != null)
                obj.SetActive(false);
        }


        // Play audio here
        if (startScreenCanvasGroup) startScreenCanvasGroup.alpha = 0;
        if (startScreenRoot != null)
            startScreenRoot.SetActive(true);

        if (startScreenCanvasGroup != null)
        {
            startScreenCanvasGroup.alpha = 0;
            startScreenCanvasGroup.DOFade(1, 3f);
        }
    }

    // Used by start screen button to continue to story sequence
    public void ContinueToStartStory()
    {
        if (startScreenCanvasGroup != null)
        {
            startScreenCanvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                if (startScreenRoot != null)
                    startScreenRoot.SetActive(false);
            });
        }
        else
        {
            if (startScreenRoot != null)
                startScreenRoot.SetActive(false);
        }

        // Call story start here, but for now just go straight to battle phase after a short delay to allow the start screen to fade out
        StartCoroutine(TutorialFlow());
    }


    public IEnumerator TutorialFlow()
    {
        if (SkipTutorial)
        {
            StartCoroutine(ReadyGameStart(1));
            yield break;
        }

        // Show tutorial UI
        // yield return new WaitForSeconds(0.5f);

        startScreenCanvasGroup.DOFade(0, 2f).OnComplete(() =>
        {
            if (startScreenRoot != null)
                startScreenRoot.SetActive(false);

            // Show tutorial root
            tutorialRoot.gameObject.SetActive(true);

            TutorialManager.Instance.StartTutorial();
        });

        // From here the TutorialManager will take over the flow and eventually call GameManager.Instance.EndTutorial() when the tutorial is complete, which will then lead into starting the battle phase.
    }


    public void EndTutorial()
    {
        // This would be called when the tutorial is completed. For now, it just hides the tutorial UI.
        tutorialRoot.DOFade(0, 0.1f).OnComplete(() =>
        {
            StartCoroutine(ReadyGameStart(0.5f));
            tutorialRoot.gameObject.SetActive(false);
        });
    }


    public IEnumerator ReadyGameStart(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Trigger story sequence here, e.g.:

        StartBattlePhase(resetSectors: true, ignoreCompass: true);

        foreach (var obj in objectsToDisableOnStartScreen)
        {   if (obj != null)
                obj.SetActive(true);
        }

        yield return new WaitForSeconds(0.3f);

        GlobalUIRootCanvasGroup.DOFade(1, 1f);
    }

}
