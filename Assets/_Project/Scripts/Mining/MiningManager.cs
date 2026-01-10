using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MiningManager : MonoBehaviour
{
    public static MiningManager Instance;
    public List<Planet> AllPlanets = new List<Planet>();
    public List<Planet> UnlockedPlanets = new List<Planet>();
    public Planet CurrentPlanet;
    public int CurrentPlanetIndex = 0;
    public Camera miningCamera;


    [Header("UI")]
    public GameObject miningUIRoot;
    public Button moveLeftButton;
    public Button moveRightButton;
    public Transform AllPlanetsContainer;


    [Header("Camera Settings")]
    public float cameraMoveSpeed = 2.0f;
    public bool isMovingCamera = false;

    [Header("Debug")]
    public int debugStartPlanetCount = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple instances of MiningManager detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AllPlanets.Clear();
        AllPlanets.AddRange(AllPlanetsContainer.GetComponentsInChildren<Planet>());


        for(int i = 0; i < debugStartPlanetCount; i++)
        {
            UnlockPlanet(AllPlanets[i]);
        }


        SetCurrentPlanet(0);

    }

    // Update is called once per frame
    void Update()
    {
        moveLeftButton.interactable = CurrentPlanetIndex > 0 && !isMovingCamera;
        moveRightButton.interactable = CurrentPlanetIndex < UnlockedPlanets.Count - 1 && !isMovingCamera;
    }

    public void SetCurrentPlanet(int planetIndex)
    {
        if (isMovingCamera)
            return;

        if (planetIndex >= 0 && planetIndex < UnlockedPlanets.Count)
        {
            CurrentPlanetIndex = planetIndex;
            CurrentPlanet = UnlockedPlanets[planetIndex];

            MoveCameraToPlanet(CurrentPlanet);
            // Additional logic to update the mining environment based on the selected planet can be added here
        }
        else
        {
            Debug.LogWarning("Invalid planet index selected.");
        }
    }


    public void UnlockPlanet(Planet planet)
    {
        if (!UnlockedPlanets.Contains(planet))
        {
            UnlockedPlanets.Add(planet);
            planet.Locked = false;
            // Additional logic for unlocking a planet can be added here
        }
    }


    public void LockPlanet(Planet planet)
    {
        if (UnlockedPlanets.Contains(planet))
        {
            UnlockedPlanets.Remove(planet);
            // Additional logic for locking a planet can be added here
        }
    }


    public void MoveCameraToPlanet(Planet planet)
    {

        isMovingCamera = true;
        if (miningCamera != null && planet != null)
        {
            Vector3 targetPosition = new Vector3(planet.transform.position.x, miningCamera.transform.position.y, planet.transform.position.z);
            miningCamera.transform.DOMove(targetPosition, cameraMoveSpeed).SetEase(Ease.InOutQuad).OnComplete(() => {
                isMovingCamera = false;
            });
        }
    }   

    public void MoveLeft()
    {
        int newIndex = CurrentPlanetIndex - 1;
        if (newIndex < 0)
        {
            Debug.Log("Already at the first planet. Cannot move left.");
            moveLeftButton.interactable = false;
            return;
        }

        moveLeftButton.interactable = true;
        SetCurrentPlanet(newIndex);
    }


    public void MoveRight()
    {
        int newIndex = CurrentPlanetIndex + 1;
        if (newIndex >= UnlockedPlanets.Count)
        {
            moveRightButton.interactable = false;
            Debug.Log("Already at the last planet. Cannot move right.");
            return;
        }

        moveRightButton.interactable = true;
        SetCurrentPlanet(newIndex);
    }
}
