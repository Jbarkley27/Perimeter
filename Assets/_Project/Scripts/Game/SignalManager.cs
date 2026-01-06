using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SignalManager : MonoBehaviour
{
    [Header("Signal Values")]
    public double startingMaxSignalSeconds = 60;
    public double maxSignalSeconds;
    public double signalSeconds;
    public double decayPerSecond = 1.0;

    [Header("UI")]
    public Slider signalSlider;
    public TMP_Text signalText;

    private bool isActive = true;
    public ParticleSystem broadcastSignalEffect;
    // public ParticleSystem broadcastSignalEffect2;

    public static SignalManager Instance { get; private set; }

    public void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // ResetSignal();
        UpdateUI();
    }

    private void Update()
    {
        if (!isActive) return;

        DecreaseSignal(Time.deltaTime * decayPerSecond);
    }

    // -----------------------------
    // PUBLIC API
    // -----------------------------

    public void ResetSignalUI()
    {
        maxSignalSeconds = startingMaxSignalSeconds;
        signalSeconds = maxSignalSeconds;
        UpdateUI();
        PauseSignal();
    }

    public void PauseSignal()
    {
        isActive = false;
    }

    public void ResumeSignal()
    {
        isActive = true;
    }

    public void IncreaseSignal(double amount)
    {
        signalSeconds += amount;
        signalSeconds = System.Math.Min(signalSeconds, maxSignalSeconds);
        UpdateUI();
    }

    public void IncreaseMaxSignal(double amount)
    {
        maxSignalSeconds += amount;
        signalSeconds = System.Math.Min(signalSeconds, maxSignalSeconds);
        UpdateUI();
    }

    public void DecreaseSignal(double amount)
    {
        if (signalSeconds <= 0)
            return;

        signalSeconds -= amount;
        signalSeconds = System.Math.Max(signalSeconds, 0);
        UpdateUI();

        if (signalSeconds <= 0)
            OnSignalDepleted();
    }

    // -----------------------------
    // INTERNAL
    // -----------------------------

    private void OnSignalDepleted()
    {
        isActive = false;
        Debug.Log("Signal depleted — run ends");

        // End the run
        GameManager.Instance.EndRun();

        // Refill signal for next run
        signalSeconds = maxSignalSeconds;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (signalSlider)
            signalSlider.value = (float)(signalSeconds / maxSignalSeconds);

        if (signalText)
            signalText.text = FormatSignalTime(signalSeconds);
    }

    private string FormatSignalTime(double seconds)
    {
        if (seconds < 60)
            return $"{seconds:F1}s";

        if (seconds < 3600)
        {
            int mins = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{mins}m {secs}s";
        }

        if (seconds < 86400)
        {
            double hours = seconds / 3600;
            return $"{hours:F1}h";
        }

        if (seconds < 86400 * 100)
        {
            double days = seconds / 86400;
            return $"{days:F1}d";
        }

        return $"{seconds:E1}s";
    }

    public IEnumerator BroadcastSignal()
    {
        if (broadcastSignalEffect != null)
        {
            broadcastSignalEffect.Play();
            // broadcastSignalEffect2.Play();
        }

        // Assume the particle effect lasts 3 seconds
        yield return new WaitForSeconds(3f);

        if (broadcastSignalEffect != null)
        {
            broadcastSignalEffect.Stop();
            // broadcastSignalEffect2.Stop();
        }

        // ResumeSignal();
    }
}


// TODO: Once we work on the Skill Tree system, we can add ways to modify the startingMaxSignalSeconds and decayPerSecond via upgrades.