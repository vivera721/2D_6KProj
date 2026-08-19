using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SceneKey = "SavedScene";
    private const string SaveXKey = "SaveX";
    private const string SaveYKey = "SaveY";
    private const string SaveZKey = "SaveZ";

    Player player;
    PlayerHealth playerHealth;
    private const string SaveMaxHPKey = "SaveMaxHP";
    private const string SaveCurrentHPKey = "SaveCurrentHP";
    private const string SaveMaxSTKey = "SaveMaxST";
    private const string SaveDMGKey = "SaveDMG";

    public bool IsContinueMode { get; private set; }


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    private void RefreshReferences()
    {
        player = FindAnyObjectByType<Player>();
        playerHealth = FindAnyObjectByType<PlayerHealth>();
    }

    public void Save(Vector3 playerPosition)
    {
        RefreshReferences();

        if (player == null || playerHealth == null)
        {
            Debug.LogWarning("Save failed: Player or PlayerHealth not found.");
            return;
        }

        PlayerPrefs.SetString(SceneKey, SceneManager.GetActiveScene().name);

        PlayerPrefs.SetFloat(SaveXKey, playerPosition.x);
        PlayerPrefs.SetFloat(SaveYKey, playerPosition.y);
        PlayerPrefs.SetFloat(SaveZKey, playerPosition.z);

        PlayerPrefs.SetInt(SaveMaxHPKey, playerHealth.maxHP);
        PlayerPrefs.SetInt(SaveCurrentHPKey, playerHealth.currentHP);
        PlayerPrefs.SetInt(SaveMaxSTKey, player.maxStamina);
        PlayerPrefs.SetFloat(SaveDMGKey, player.attackDamage);
        PlayerPrefs.Save();

        Debug.Log($"Saved Position: {playerPosition}");
    }

    public int LoadMaxHP()
    {
        return PlayerPrefs.GetInt(SaveMaxHPKey, 0);
    }
    public int LoadCurrentHP()
    {
        return PlayerPrefs.GetInt(SaveCurrentHPKey, 0);
    }
    public int LoadStamina()
    {
        return PlayerPrefs.GetInt(SaveMaxSTKey, 0);
    }
    public float LoadDMG()
    {
        return PlayerPrefs.GetFloat(SaveDMGKey, 0f);
    }

    public string LoadSceneName()
    {
        return PlayerPrefs.GetString(SceneKey, "");
    }

    public Vector3 Load()
    {
        float x = PlayerPrefs.GetFloat(SaveXKey, 0f);
        float y = PlayerPrefs.GetFloat(SaveYKey, 0f);
        float z = PlayerPrefs.GetFloat(SaveZKey, 0f);

        Vector3 loadedPosition = new Vector3(x, y, z);

        Debug.Log($"Loaded Position: {loadedPosition}");
        return loadedPosition;
    }

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SceneKey)
            && PlayerPrefs.HasKey(SaveXKey)
            && PlayerPrefs.HasKey(SaveYKey)
            && PlayerPrefs.HasKey(SaveZKey);
    }
    public void SetContinueMode(bool value)
    {
        IsContinueMode = value;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SceneKey);
        PlayerPrefs.DeleteKey(SaveXKey);
        PlayerPrefs.DeleteKey(SaveYKey);
        PlayerPrefs.DeleteKey(SaveZKey);

        PlayerPrefs.DeleteKey(SaveMaxHPKey);
        PlayerPrefs.DeleteKey(SaveCurrentHPKey);
        PlayerPrefs.DeleteKey(SaveMaxSTKey);
        PlayerPrefs.DeleteKey(SaveDMGKey);
        PlayerPrefs.Save();
    }
}
