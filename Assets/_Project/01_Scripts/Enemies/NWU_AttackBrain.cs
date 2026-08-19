using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NWU_AttackBrain : MonoBehaviour, IEnemyBrain
{
    public EnemyAttackProfile attackProfile;

    [Header("Attack Range")]
    [Space(10)]
    public float meleeRange = 1.5f;          // 공격 사거리
    public float SlamAttackRange = 1.5f;

    [Header("Sweep Range")]
    [Space(10)]
    public float SweepAttackRange = 4f;

    [Header("Beam Range")]
    [Space(10)]
    public float beamRange;

    private void Awake()
    {
        beamRange = attackProfile.beamRange;
    }

    public void Tick(EnemyCore core, float dt)
    {
        if (core == null) return;

        // 타겟 없으면 그냥 정지(혹은 이동 켜도 되지만, 보통 정지 추천)
        if (core.target == null)
        {
            core.Movement?.SetEnabled(false);
            return;
        }

        // =====================================================

        var attack = core.Attack as WakeUpDashAttack;

        if (attack == null)
        {
            core.Movement?.SetEnabled(true);
            return;
        }

        // 공격 중이면 정지
        if (core.Attack != null && core.Attack.IsAttacking)
        {
            core.Movement?.SetEnabled(false);
            return;
        }

        float dist = Vector2.Distance(core.transform.position, core.target.position);

        core.SetFacing(core.target.position.x - core.transform.position.x);

        // Heal 우선
        if (attackProfile != null && attackProfile.enableHeal && attack.CanUseHealPublic(core))
        {
            core.Movement?.SetEnabled(false);
            attack.TryStartAttackExternal(WakeUpDashAttack.AttackType.Heal, core);
            return;
        }

        // Beam 우선 범위
        if (attackProfile != null && attackProfile.enableBeam &&
            dist <= beamRange && dist > meleeRange)
        {
            core.Movement?.SetEnabled(false);
            attack.TryStartAttackExternal(WakeUpDashAttack.AttackType.Beam, core);
            return;
        }

        // Melee 근거리
        if (attackProfile != null &&
            attackProfile.enableMelee &&
            dist <= meleeRange)
        {
            core.Movement?.SetEnabled(false);
            attack.TryStartAttackExternal(WakeUpDashAttack.AttackType.Melee, core);
            return;
        }

        // Sweep 범위
        if (attackProfile != null &&
            attackProfile.enableSweep &&
            dist <= SweepAttackRange)
        {
            core.Movement?.SetEnabled(false);
            attack.TryStartAttackExternal(WakeUpDashAttack.AttackType.Sweep, core);
            return;
        }

        // 그 외에는 이동
        core.Movement?.SetEnabled(true);

        // Chase / Attack 판단
        //if (dist > meleeRange)
        //{
        //    // 공격 사거리 밖 => Chase (patrol X)
        //    core.Movement?.SetEnabled(true);
        //    return;
        //}
        //else
        //{
        //    // 공격 사거리 안 => 멈추고 공격
        //    core.Movement?.SetEnabled(false);

        //    if (core.Attack != null && core.Attack.CanAttack(core))
        //    {
        //        float dx = core.target.position.x - core.transform.position.x;
        //        core.SetFacing(dx); // 공격 시작 순간 방향 고정
        //        core.Attack.Execute(core);
        //    }

        //    return;
        //}

    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, SlamAttackRange);

        Gizmos.color = Color.violet;
        Gizmos.DrawWireSphere(transform.position, SweepAttackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, beamRange);

    }

#endif
}