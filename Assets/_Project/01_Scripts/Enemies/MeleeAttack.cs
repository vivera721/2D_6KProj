using UnityEngine;

public class MeleeAttack : MonoBehaviour, IEnemyAttack
{
    public float attackCooltime = 2f;
    public float damage = 10f;

    float lastAttackTime = -999f;
    public bool IsAttacking { get; private set; }

    public float attackLockTime = 0.4f;

    public bool CanAttack(EnemyCore core)
    {
        if (IsAttacking) return false;

        return Time.time > lastAttackTime + attackCooltime;
    }

    public void Execute(EnemyCore core)
    {
        lastAttackTime = Time.time;
        IsAttacking = true;


        core.animator.SetTrigger("Attack");
        Debug.Log("Enemy Attack!");

        CancelInvoke(nameof(ForceEndAttack));
        Invoke(nameof(ForceEndAttack), attackLockTime);


    }

    public void Tick(EnemyCore core, float dt)
    {
        // 공격 조건 / 쿨타임
    }

    public void OnAttackAnimationStart()
    {
        transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = true;
        //GetComponentInChildren<BoxCollider2D>().enabled = true;
    }

    public void OnAttackAnimationEnd()
    {
        Debug.Log("Attack Animation End Event!", this);
        IsAttacking = false;
        transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
        //GetComponentInChildren<BoxCollider2D>().enabled = false;
    }

    void ForceEndAttack()
    {
        IsAttacking = false;
    }

}
