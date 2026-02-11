using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class TutorialManager : MonoBehaviour 
{
    public static TutorialManager Instance { get; private set; }
    public CanvasGroup tutorialRoot;
    public List<CanvasGroup> tutorialSteps = new List<CanvasGroup>();
    public int currentStepIndex = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;

    }

    public void Enable()
    {
        // Subscribe to any events if needed
        tutorialRoot.alpha = 1;
    }

    public void StartTutorial()
    {
        Debug.Log("Starting Tutorial");
        // Start the tutorial flow, which could include showing dialogues, objectives, etc. For now, it just starts a coroutine that simulates the tutorial.
        currentStepIndex = 0;
        tutorialRoot.alpha = 1;
        // Show the first tutorial step after the root has faded in
        ShowNextTutorialStep();
    }

    public void EndTutorial()
    {
        Debug.Log("Ending Tutorial");
        // This would be called when the tutorial is completed. For now, it just hides the tutorial UI.
        tutorialRoot.DOFade(0, 0.1f).OnComplete(() =>
        {
            GameManager.Instance.EndTutorial();
            tutorialRoot.gameObject.SetActive(false);
        });
    }

    public void ShowNextTutorialStep()
    {
        if (currentStepIndex < tutorialSteps.Count)
        {
            // Hide all steps first
            foreach (var step in tutorialSteps)
            {
                step.alpha = 0;
                step.gameObject.SetActive(false);
            }

            // Show the current step
            var currentStep = tutorialSteps[currentStepIndex];
            currentStep.gameObject.SetActive(true);
            currentStep.DOFade(1, 1f);
            currentStepIndex++;
        }
        else
        {
            EndTutorial();
        }
    }
}