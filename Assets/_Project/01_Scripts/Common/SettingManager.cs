using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private Slider bgmSlider;

    private const string BGMVolumeKey = "BGMVolume";

    [Header("Screen")]
    [SerializeField] private Toggle fullscreenToggle;

    private const string FullScreenKey = "Fullscreen";

    private void Start()
    {
        LoadFullScreenSetting();
        LoadBGMVolume();

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullScreen);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
    }

    private void OnEnable()
    {
        LoadBGMVolume();
        LoadFullScreenSetting() ;
    }

    private void OnDestroy()
    {
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(SetFullScreen);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(SetBGMVolume);
    }
    private void LoadFullScreenSetting()
    {
        bool isFullscreen = PlayerPrefs.GetInt(FullScreenKey, 1) == 1;

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);

        ApplyFullScreen(isFullscreen);
    }

    public void SetFullScreen(bool isFullscreen)
    {
        ApplyFullScreen(isFullscreen);

        PlayerPrefs.SetInt(FullScreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyFullScreen(bool isFullscreen)
    {
        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }
    }

    private void LoadBGMVolume()
    {
        if (BGMManager.Instance == null) return;

        float currentVolume = BGMManager.Instance.Volume;

        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(currentVolume);
    }

    public void SetBGMVolume(float volume)
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.SetVolume(volume);
    }
}
