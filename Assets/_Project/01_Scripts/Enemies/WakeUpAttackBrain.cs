using System.Collections;
using UnityEngine;

public class WakeUpAttackBrain : MonoBehaviour, IEnemyBrain
{
    [Header("Detect & Attackable Range")]
    public float detectRange = 10f;        // 감지 거리 (공격범위보다 넓어야 함)

    public float attackRange = 1.5f;          // 공격 사거리

    [Header("WakeUp")]
    //private bool isSleep = true;

    private bool wakeUp = false;  // WakeUp 트리거 1회용
    private bool canMove = false; // WakeUp 애니 끝나면 true (Animation Event)

    [Header("Attack Range")]
    public float SlamAttackRange = 1.5f;

    public float SweepAttackRange = 4f;

    public void Tick(EnemyCore core, float dt)
    {
        if (core == null) return;

        // 타겟 없으면 그냥 정지(혹은 이동 켜도 되지만, 보통 정지 추천)
        if (core.target == null)
        {
            core.Movement?.SetEnabled(false);
            return;
        }

        float dist = Vector2.Distance(core.transform.position, core.target.position);

        // 1) Sleep 판단: DetectDistance 밖이면 Sleep
        bool shouldSleep = dist > detectRange;

        // 잠자는 상태면 무조건 정지
        if (shouldSleep)
        {
            // 수면 상태: 무조건 정지
            core.Movement?.SetEnabled(false);

            // "다시 잠드는 기능은 필요없음" 이라면 아래 3줄 주석 처리하면 됨.
            /*
            isSleep = true;
            wakeUp = false;
            canMove = false;
            */

            return;
        }

        // 감지 범위 안에 들어오면 깨어남 - 애니메이션 한번만 재생
        if (!wakeUp)
        {
            core.Movement?.SetEnabled(false); // 기상 중에는 정지
            core.animator.SetTrigger("WakeUp");
            wakeUp = true;
            return;
        }

        // WakeUp 애니 재생 중이면 정지 (애니 이벤트에서 WakeUpStatus() 호출)
        if (!canMove)
        {
            core.Movement?.SetEnabled(false);
            return;
        }

        // =====================================================

        // 공격 중이면 정지
        if (core.Attack != null && core.Attack.IsAttacking)
        {
            core.Movement?.SetEnabled(false);
            return;
        }

        // Chase / Attack 판단
        if (dist > attackRange)
        {
            // 공격 사거리 밖 => Chase (patrol X)
            core.Movement?.SetEnabled(true);
            return;
        }
        else
        {
            // 공격 사거리 안 => 멈추고 공격
            core.Movement?.SetEnabled(false);

            if (core.Attack != null && core.Attack.CanAttack(core))
            {
                float dx = core.target.position.x - core.transform.position.x;
                core.SetFacing(dx); // 공격 시작 순간 방향 고정
                core.Attack.Execute(core);
            }

            return;
        }
    }

    public void WakeUpStatus()
    {
        canMove = true;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, SlamAttackRange);

        Gizmos.color = Color.violet;
        Gizmos.DrawWireSphere(transform.position, SweepAttackRange);
    }

#endif
}