using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string NewGameScene;
    private string ContinueGameScene;

    [SerializeField]private DOTweenAnimation fadeAnimation;

    [Header("Button")]
    [SerializeField] private Button continueButton;

    private bool isNewGame;
    private bool isChangingScene;

    private void Start()
    {
        RefreshContinueButton();
    }

    private void RefreshContinueButton()
    {
        bool hasSaveData = SaveManager.Instance != null && SaveManager.Instance.HasSaveData();

        if (continueButton != null)
            continueButton.interactable = hasSaveData;
    }

    public void NewGame()
    {
        if (isChangingScene) return;
        isChangingScene = true;

        isNewGame = true;

        if(SaveManager.Instance != null)
            SaveManager.Instance.SetContinueMode(false);

        Time.timeScale = 1f;
        fadeAnimation.DORestart();
        //SceneManager.LoadScene(NewGameScene);
    }

    public void ContinueScene()
    {
        if (isChangingScene) return;

        isNewGame = false;

        if (SaveManager.Instance == null || !SaveManager.Instance.HasSaveData())
        {
            Debug.Log("No Save Data");
            return;
        }

        ContinueGameScene = SaveManager.Instance.LoadSceneName();

        if (string.IsNullOrEmpty(ContinueGameScene))
            return;

        isChangingScene = true;

        SaveManager.Instance.SetContinueMode(true);

        Time.timeScale = 1f;
        fadeAnimation.DORestart();
        //SceneManager.LoadScene(sceneName);
    }

    public void GoToNextScene()
    {
        if (isNewGame)
            SceneManager.LoadScene(NewGameScene);
        else
        {
            if (string.IsNullOrEmpty(ContinueGameScene))
            {
                Debug.LogWarning("ContinueGameScene is empty.");
                isChangingScene = false;
                return;
            }

            SceneManager.LoadScene(ContinueGameScene);
        }
    }

    public void GoBackScene()
    {
        SceneManager.LoadScene("Main Menu");
    }
    public void SettingScene()
    {
        Debug.Log("Load Setting Scene");
        //SceneManager.LoadScene("World");
    }
    public void Quit()
    {
        Debug.Log("Quit Button Activated");
        Application.Quit();
        //SceneManager.LoadScene("World");
    }
    public void CreditScene()
    {
        Debug.Log("Load Credit Scene");
        //SceneManager.LoadScene("World");
    }

}
