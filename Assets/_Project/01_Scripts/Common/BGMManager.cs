using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    AudioSource audioSource;

    private const string BGMVolumeKey = "BGMVolume";

    [Header("BGM_Clip")]
    [SerializeField] private AudioClip Stage1_BGM;
    [SerializeField] private AudioClip Stage1_Boss_BGM;
    [SerializeField] private AudioClip Stage2_BGM;
    [SerializeField] private AudioClip Stage2_Boss_BGM;
    [SerializeField] private AudioClip Stage3_BGM;
    [SerializeField] private AudioClip Stage3_Boss_BGM;
    [SerializeField] private AudioClip MainMenu_BGM;
    [SerializeField] private AudioClip GameEnding_BGM;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float volume = 0.25f;

    public float Volume => volume;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(audioSource == null)
            audioSource = GetComponent<AudioSource>();

        LoadVolume();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayStageBGM(scene.name);
    }
    public void PlayStageBGM(string sceneName)
    {
        AudioClip clipToPlay = null;

        switch (sceneName)
        {
            case "Main Menu":
                clipToPlay = MainMenu_BGM;
                break;

            case "Stage1":
                clipToPlay = Stage1_BGM;
                break;

            case "Stage2":
                clipToPlay = Stage2_BGM;
                break;

            case "Stage3":
                clipToPlay = Stage3_BGM;
                break;

            case "FinalScene":
                clipToPlay = GameEnding_BGM;
                break;
        }

        PlayBGM(clipToPlay);
    }
    public void PlayBossBGM()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        AudioClip clipToPlay = null;

        switch (sceneName)
        {
            case "Stage1":
                clipToPlay = Stage1_Boss_BGM;
                break;

            case "Stage2":
                clipToPlay = Stage2_Boss_BGM;
                break;

            case "Stage3":
                clipToPlay = Stage3_Boss_BGM;
                break;
        }

        PlayBGM(clipToPlay);
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private void LoadVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(BGMVolumeKey, volume);
        SetVolume(savedVolume);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (audioSource != null)
            audioSource.volume = volume;

        PlayerPrefs.SetFloat(BGMVolumeKey, volume);
        PlayerPrefs.Save();
    }
}
