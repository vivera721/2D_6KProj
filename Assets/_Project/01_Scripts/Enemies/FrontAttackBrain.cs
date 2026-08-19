using System.Collections;
using UnityEngine;

public class FrontAttackBrain : MonoBehaviour, IEnemyBrain
{
    public float attackRange = 1.5f;

    [SerializeField] private float lostSightWait = 0.5f;
    private Coroutine lostSightCo;
    private bool lostSightHandled;

    [Header("Sight (Raycast)")]
    public float sightDistance = 3f;          // 눈앞 시야 거리

    public float sightHalfAngle = 25f;        // 시야 반각 (좌우 합쳐서 50도)
    public LayerMask sightBlockLayers;        // 벽/지형
    public LayerMask targetLayers;            // Player
    public Transform eye;                     // 눈 위치 (없으면 enemy 루트)

    public void Tick(EnemyCore core, float dt)
    {
        if (core.target == null)
        {
            core.Movement?.SetEnabled(true);
            return;
        }

        bool canSee = CanSeeTarget(core);
        if (canSee) lostSightHandled = false;   // 다시 보이면 다음번에 또 처리 가능

        float dist = Vector2.Distance(core.transform.position, core.target.position);

        // 1) 공격 중이면 정지
        if (core.Attack != null && core.Attack.IsAttacking)
        {
            core.Movement?.SetEnabled(false);
            return;
        }

        // 시야 안 + 공격범위 밖 = 이동
        if (canSee && dist > attackRange)
        {
            StopLostSightCoroutineIfAny();
            core.Movement?.SetEnabled(true);
            return;
        }
        // 시야 안 + 공격범위 안 = 멈춤(공격 가능하면 공격)
        if (canSee && dist <= attackRange)
        {
            StopLostSightCoroutineIfAny();
            core.Movement?.SetEnabled(false);

            if (core.Attack != null && core.Attack.CanAttack(core))
            {
                float dx = core.target.position.x - core.transform.position.x;
                core.SetFacing(dx); // 공격 시작 순간 방향 고정
                core.Attack.Execute(core);
            }

            return;
        }

        // 공격범위 안에서 시야 잃음 ( 공격대상 잃음 ) = 0.5초 대기 후 이동
        if (!canSee && dist <= attackRange)
        {
            // 시야 잃음 처리 "딱 1번만"
            if (!lostSightHandled && lostSightCo == null)
            {
                lostSightHandled = true;
                lostSightCo = StartCoroutine(MoveInterval(core, lostSightWait));
            }

            // 0.5초 대기 중이면 멈춤 유지
            if (lostSightCo != null)
            {
                core.Movement?.SetEnabled(false);
                return;
            }

            // 0.5초 대기 끝났으면: 더 이상 멈춤 재발동 금지, 그냥 이동(패트롤)로
            core.Movement?.SetEnabled(true);
            return;
        }

        // 그 외 (시야 밖 + 공격범위 밖) = 기본이동(patrol)
        StopLostSightCoroutineIfAny();
        core.Movement?.SetEnabled(true);
    }

    private void StopLostSightCoroutineIfAny()
    {
        if (lostSightCo != null)
        {
            StopCoroutine(lostSightCo);
            lostSightCo = null;
        }
    }

    private IEnumerator MoveInterval(EnemyCore core, float time)
    {
        core.Movement?.SetEnabled(false);
        Debug.Log("시야 상실, 대기 시작", this);
        yield return new WaitForSeconds(time);

        Debug.Log(CanSeeTarget(core) ? "대기 후 재발견" : "대기 후에도 미발견 → 이동", this);
        core.Movement?.SetEnabled(true);
        lostSightCo = null;
    }

    private bool CanSeeTarget(EnemyCore core)
    {
        Vector3 origin = eye ? eye.position : core.transform.position;
        Vector3 targetPos = core.target.position;

        Vector2 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;
        if (dist > sightDistance) return false;

        // 전방 벡터 (Facing이 +1/-1이 아닐 수도 있으니 보정)
        float facing = core.Facing;
        if (Mathf.Abs(facing) < 0.01f) facing = 1f; // 0 방지
        Vector2 forward = new Vector2(Mathf.Sign(facing), 0f);

        // 타겟이 전방이 아니면 바로 컷 (뒤면 무조건 false)
        if (Vector2.Dot(forward, toTarget.normalized) <= 0f)
            return false;

        // 시야각 체크(옵션이지만 유지)
        float angle = Vector2.Angle(forward, toTarget);
        if (angle > sightHalfAngle) return false;

        // Raycast (LOS 체크)
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            toTarget.normalized,
            dist,
            sightBlockLayers | targetLayers
        );

        //Debug.Log($"Facing={core.Facing}, dot={Vector2.Dot(forward, toTarget.normalized)}, angle={angle}");

        return hit.collider != null && hit.transform == core.target;
    }

#if UNITY_EDITOR
    //void OnDrawGizmosSelected()
    //{
    //    var core = GetComponent<EnemyCore>();
    //    if (core == null) return;

    //    Vector3 origin = eye ? eye.position : transform.position;
    //    Vector3 forward = new Vector3(core.Facing, 0f, 0f);

    //    Vector3 leftDir = Quaternion.Euler(0, 0, sightHalfAngle) * forward;
    //    Vector3 rightDir = Quaternion.Euler(0, 0, -sightHalfAngle) * forward;

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawLine(origin, origin + forward * sightDistance);
    //    Gizmos.DrawLine(origin, origin + leftDir * sightDistance);
    //    Gizmos.DrawLine(origin, origin + rightDir * sightDistance);

    //    Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
    //    Gizmos.DrawWireSphere(origin, sightDistance);
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        var core = GetComponent<EnemyCore>();
        if (core == null) return;

        Vector3 origin = eye ? eye.position : transform.position;
        Vector3 forward = new Vector3(core.Facing, 0f, 0f);

        Vector3 leftDir = Quaternion.Euler(0, 0, sightHalfAngle) * forward;
        Vector3 rightDir = Quaternion.Euler(0, 0, -sightHalfAngle) * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + forward * sightDistance);
        Gizmos.DrawLine(origin, origin + leftDir * sightDistance);
        Gizmos.DrawLine(origin, origin + rightDir * sightDistance);

        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(origin, sightDistance);
    }

#endif
}