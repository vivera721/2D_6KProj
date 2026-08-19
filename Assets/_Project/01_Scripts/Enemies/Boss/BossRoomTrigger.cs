using DG.Tweening;
using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    private BoxCollider2D roomCollider;

    [SerializeField] private BossBattleManager bossBattleManager;
    //[SerializeField] private DOTweenAnimation anim;
    RoomManager roomManager;

    private bool triggered;

    private void Awake()
    {
        roomCollider = GetComponent<BoxCollider2D>();

        roomManager = GetComponentInParent<RoomManager>();

        //if (anim == null)
        //    anim = GetComponent<DOTweenAnimation>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (!collision.CompareTag("Player")) return;

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayBossBGM();

        triggered = true;

        //if (anim != null)
        //    anim.DOPlay();

        if (bossBattleManager != null)
            bossBattleManager.StartBossIntro(collision.transform);

        if (roomManager != null)
            roomManager.StartRoom();

        Debug.Log("Boss Room Entered");
    }
}