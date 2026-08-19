using UnityEngine;

public class StageFlowManager : MonoBehaviour
{
    public int stageIndex;
    public int clearedRoomCount;
    public int requiredClearRoomCount = 3;

    [SerializeField] private string nextStageSceneName;

    [SerializeField] private Portal portal;

    private void Awake()
    {
        if (portal != null)
            portal.gameObject.SetActive(false);
    }

    public void OnRoomCleared(RoomManager room)
    {
        clearedRoomCount++;

        if(room.RoomType == RoomType.Boss)
        {
            // Boss Cleared, load next stage immediately
            OnBossRoomCleared();
            return;
        }

        if (clearedRoomCount >= requiredClearRoomCount)
        {
            // Can Go to Boss Room
            // 여기서 보스방 문 열기 등을 처리하면 됨
        }
    }

    public void OnBossRoomCleared()
    {
        // Boss Clear Prize or Upgrade Activation
        // Next Stage Portal or Door Activation

        if (portal != null)
            portal.gameObject.SetActive(true);
    }
    public void LoadNextStage()
    {
        if (string.IsNullOrEmpty(nextStageSceneName))
        {
            Debug.LogWarning("Next Stage Scene Name is empty.");
            return;
        }
        SaveRuntimePlayerData();
        // Load next stage scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextStageSceneName);
    }
    private void SaveRuntimePlayerData()
    {
        Player player = FindAnyObjectByType<Player>();
        PlayerHealth health = FindAnyObjectByType<PlayerHealth>();

        if (PlayerRuntimeData.Instance != null)
            PlayerRuntimeData.Instance.SaveFromPlayer(player, health);
    }
}
