using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (GameManager.Instance.GamePaused)
        {
            Destroy(gameObject);
        }
    }
}