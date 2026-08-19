using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    public void NextScene()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
