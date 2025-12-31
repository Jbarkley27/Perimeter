using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TMP_Text fpsText;

    private float deltaTime = 0.0f;

    void Start()
    {
        #if !DEVELOPMENT_BUILD
            gameObject.SetActive(true);
        #endif
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = Mathf.Ceil(fps).ToString() + " FPS";
    }
}
