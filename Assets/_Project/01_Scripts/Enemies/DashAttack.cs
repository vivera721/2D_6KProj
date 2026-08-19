using System.Collections;
using UnityEngine;

public class DashAttack : MonoBehaviour, IEnemyAttack
{
    [Header("Cooldown")]
    public float attackCooltime = 2f;

    [Header("Dash")]
    public float dashSpeed = 6f;

    public float dashDuration = 0.25f;
    public float dashStopDistance = 0.1f; // (선택) 타겟 근처 멈추기

    private float lastAttackTime = -999f;
    private float dashTimer;

    [Header("WindUp")]
    public float windUpTime = 0.5f;

    private bool canDash;
    private Coroutine windUpCo;

    public bool IsAttacking { get; private set; }

    public bool CanAttack(EnemyCore core)
    {
        return Time.time >= lastAttackTime + attackCooltime;
    }

    public void Execute(EnemyCore core)
    {
        lastAttackTime = Time.time;
        IsAttacking = true;
        dashTimer = 0f;
        canDash = false;

        // ? 돌진 시작 순간 방향 고정
        if (core.target != null)
        {
            float dx = core.target.position.x - core.transform.position.x;
            core.SetFacing(dx);
        }

        if (windUpCo != null) StopCoroutine(windUpCo);

        windUpCo = StartCoroutine(WindUp(core));

        core.animator?.SetTrigger("Dash"); // 애니 있으면
    }

    public void Tick(EnemyCore core, float dt)
    {
        if (!IsAttacking) return;

        if (!canDash)
        {
            core.Movement?.SetEnabled(false);
            return;
        }

        dashTimer += dt;

        // 돌진 이동 (Rigidbody 없으니 transform 이동)
        Vector3 dir = new Vector3(core.Facing, 0f, 0f);
        core.transform.position += dir * dashSpeed * dt;

        // 시간 종료
        if (dashTimer >= dashDuration)
        {
            IsAttacking = false;
            canDash = false;
            windUpCo = null;
        }
    }

    private IEnumerator WindUp(EnemyCore core)
    {
        core.Movement?.SetEnabled(false);

        Debug.Log("Dash 전 WindUp 0.5초 대기");

        yield return new WaitForSeconds(windUpTime);

        canDash = true;
    }
}