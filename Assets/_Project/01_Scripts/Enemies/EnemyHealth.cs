using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyAudio))]
public class EnemyHealth : MonoBehaviour
{
    // HP
    // Hit / Invincible
    // Death 


    public float maxHp;
    public float currentHp;

    public float MaxHp => maxHp;
    public float CurrentHP => currentHp;
    EnemyCore core;

    EnemyHitFlashMaterialSwap hitFlash;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private EnemyAudio enemyAudio;

    private void Awake()
    {
        core = GetComponent<EnemyCore>();
        hitFlash = GetComponent<EnemyHitFlashMaterialSwap>();
        currentHp = maxHp;
        roomManager = GetComponentInParent<RoomManager>();
        enemyAudio = GetComponent<EnemyAudio>();
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp > 0)
        {
            enemyAudio.PlayHit();
            //core.animator.SetTrigger("Damaged");
            hitFlash.PlayFlash();
        }
        //Debug.Log("Enemy Hit!");

        else if(currentHp <= 0)
        {
            if (HasTriggerParameter("Die"))
            {
                core.animator.SetTrigger("Die");
                DisableComponents();
            }
            else
            {
                StartCoroutine(DieCoroutine());
            }
            //Die();
            // 애니메이션 끝나면 destroy 할 수 있게? or collider 만 disable 하고 맵 넘어갈때 삭제?
        }
    }

    IEnumerator DieCoroutine()
    {
        DisableComponents();
        yield return new WaitForSeconds(0.5f);
        Die();
    }

    private bool HasTriggerParameter(string paramName)
    {
        if (core.animator == null) return false;

        foreach (AnimatorControllerParameter param in core.animator.parameters)
        {
            if (param.name == paramName &&
                param.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (currentHp <= 0) return;

        currentHp += amount;
        if (currentHp > maxHp)
            currentHp = maxHp;
    }

    void DisableComponents()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        core.Movement?.SetEnabled(false);
        core.enabled = false;
    }

    public void Die()
    {
        roomManager.CheckEnemyDead(gameObject);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("AttackCollider")) return;

        if (collision.TryGetComponent(out DamageDealer dealer))
        {
            if (dealer.ownerType != DamageOwner.Player) return;

            TakeDamage(dealer.damage);

            Debug.Log("dealer.owner = " + dealer.owner);

            // 일반 적만 약간 넉백
            if (core != null)
                KnockbackFrom(dealer.owner.position, 1.5f);
        }
    }
    void KnockbackFrom(Vector3 attackerPos, float force)
    {
        // 보스는 제외하고 싶으면 BossBase가 있으면 return
        if (GetComponent<BossBase>() != null)
            return;

        float dir = transform.position.x > attackerPos.x ? 1f : -1f;

        transform.position += new Vector3(dir * force * Time.deltaTime, 0f, 0f);
    }
}
