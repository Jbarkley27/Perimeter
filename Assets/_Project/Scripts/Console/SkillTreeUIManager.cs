using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUIManager : MonoBehaviour
{
    public static SkillTreeUIManager Instance;

    public ScrollRect scrollRect;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found a SkillTreeUIManager object, destroying new one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // public void CenterUIOnScreen()
    // {
    //     // Implement centering logic if needed
    //     scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);

    // }

}