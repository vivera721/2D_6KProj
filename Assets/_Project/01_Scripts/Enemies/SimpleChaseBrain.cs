using UnityEngine;

public class SimpleChaseBrain : MonoBehaviour, IEnemyBrain
{
    public float attackRange = 1.5f;

    public void Tick(EnemyCore core, float dt)
    {
        if (core.target == null)
        {
            core.Movement?.SetEnabled(true);
            return;
        }
        float dist = Vector2.Distance(core.transform.position, core.target.position);

        // 1) 공격 애니메이션 중이면 무조건 멈춤
        if (core.Attack != null && core.Attack.IsAttacking)
        {
            core.Movement?.SetEnabled(false);
            return;
        }

        // 2) Range 밖이면 무조건 이동(플레이어가 벗어나면 다시 움직이길 원함)
        if(dist > attackRange)
        {
            core.Movement?.SetEnabled(true);
            return;
        }

        // 3) Range 안이면: 쿨타임 끝나면 공격, 아니면 대기(멈춤)
        if (core.Attack != null && core.Attack.CanAttack(core))
        {
            Vector3 toTarget = core.target.position - core.transform.position;
            core.SetFacing(toTarget.x); 

            core.Movement?.SetEnabled(false);
            core.Attack.Execute(core);
        }
        else
        {
            // range 안 + cooltime = 대기
            core.Movement?.SetEnabled(false);
        }
    }
}
