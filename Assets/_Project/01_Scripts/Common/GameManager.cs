using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState currentState { get; private set; } = GameState.Playing;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject firstSelectedButton;


    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if(pauseMenu != null) 
            pauseMenu.SetActive(false);

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        Debug.Log("PauseGame Called");
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;
        SetState(GameState.Pause);

        if (pauseMenu != null)
        {
            Debug.Log("PauseMenu Active True");
            pauseMenu.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PauseMenu is NULL");
        }

        // Set first selected button for gamepad navigation
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;
        SetState(GameState.Exploration);

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        // Set first selected button for gamepad navigation
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        // Handle state-specific logic here
    }
    public bool IsPlayerControlAllowed()
    {
        return currentState == GameState.Exploration || currentState == GameState.BossBattle;
    }

    public GameState GetGameState()
    {
        return currentState;
    }
}