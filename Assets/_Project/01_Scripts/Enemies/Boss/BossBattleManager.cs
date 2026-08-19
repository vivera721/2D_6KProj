using System.Collections;
using UnityEngine;

public class BossBattleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private BossBase boss;
    [SerializeField] private CameraTargetController cameraTargetController;
    [SerializeField] private Transform playerCameraTarget;
    [SerializeField] private Transform bossCameraTarget;

    [Header("Intro Settings")]
    [SerializeField] private float bossFocusTimeBeforeWakeUp = 1f;
    [SerializeField] private float playerFocusTimeAfterWakeUp = 0.5f;
    [SerializeField] private float battleStartDelay = 0.5f;
    [SerializeField] private float bossIntroDuration = 1.2f;

    [SerializeField] private BossIntroUI bossIntroUI;

    private bool battleStarted;

    public void StartBossIntro(Transform playerTransform)
    {
        if (battleStarted) return;

        battleStarted = true;

        if (boss != null)
            boss.SetTarget(playerTransform);

        StartCoroutine(BossIntroRoutine());
    }

    private IEnumerator BossIntroRoutine()
    {
        GameManager.Instance.SetState(GameState.BossIntro);

        // 1. 플레이어 정지
        player.SetControlEnabled(false);
        player.StopImmediately();
        player.PrepareForBossIntro();

        yield return new WaitForSeconds(0.5f);

        // 2. 카메라 보스 포커스
        cameraTargetController.SetFollowTarget(bossCameraTarget);

        yield return new WaitForSeconds(bossFocusTimeBeforeWakeUp);

        bossIntroUI.PlayIntro();
        yield return new WaitForSeconds(bossIntroDuration);

        // 3. 보스 WakeUp 시작
        boss.OnWakeUpFinished += HandleBossWakeUpFinished;
        boss.PlayWakeUp();
    }

    private void HandleBossWakeUpFinished()
    {
        boss.OnWakeUpFinished -= HandleBossWakeUpFinished;
        StartCoroutine(StartBattleRoutine());
    }

    private IEnumerator StartBattleRoutine()
    {
        yield return new WaitForSeconds(playerFocusTimeAfterWakeUp);

        // 4. 카메라 다시 플레이어 포커스
        cameraTargetController.SetFollowTarget(playerCameraTarget);

        yield return new WaitForSeconds(battleStartDelay);

        // 5. 플레이어 조작 허용
        player.SetControlEnabled(true);

        // 6. 보스 AI 시작
        boss.StartBattle();

        GameManager.Instance.SetState(GameState.BossBattle);

        // 나중에 추가 가능
        // BossUI.Show(boss);
        // BGMManager.PlayBossBGM();
        // BossDoor.Close();
    }
}