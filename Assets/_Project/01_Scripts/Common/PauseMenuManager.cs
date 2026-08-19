using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    public void OnClickResume()
    {
        GameManager.Instance.ResumeGame();
    }

    public void OnClickReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnClickQuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private void OnEnable()
    {
        MenuButtonSelector[] selectors = GetComponentsInChildren<MenuButtonSelector>(true);

        foreach (MenuButtonSelector selector in selectors)
        {
            selector.HideSelector();
        }
    }
}
