using System.Linq;
using UnityEngine;
[RequireComponent(typeof(EnemyAudio))]
public class BossHealth : MonoBehaviour
{
    public float maxHealth = 100;
    public float currentHealth;
    
    BossHitFlash hitFlash;
    Animator animator;
    //GolemState state;

    [SerializeField] private RoomManager roomManager;

    [SerializeField]private EnemyAudio enemyAudio;


    private void Awake()
    {
        currentHealth = maxHealth;
        hitFlash = GetComponent<BossHitFlash>();
        animator = GetComponent<Animator>();
        roomManager = GetComponentInParent<RoomManager>();
        enemyAudio = GetComponent<EnemyAudio>();

    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth > 0)
        {
            //core.animator.SetTrigger("Damaged");
            enemyAudio.PlayHit();
            hitFlash.PlayFlash();
        }
        //Debug.Log("Enemy Hit!");

        else if(currentHealth <= 0)
        {
            //enemyAudio.PlayHit();
            animator.SetTrigger("Die");
            //DisableComponents();
            // 애니메이션 끝나면 destroy 할 수 있게? or collider 만 disable 하고 맵 넘어갈때 삭제?
        }
    }
    public void Heal(int amount) // not used in this project
    {
        if (amount <= 0) return;
        if (currentHealth <= 0) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void DisableComponents()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<BossHitFlash>().enabled = false;

        if(GetComponent<BossContactDamage>() != null) 
            GetComponent<BossContactDamage>().enabled = false;

        GetComponentsInChildren<GolemAttackHitbox>().ToList().ForEach(hitbox => hitbox.enabled = false);

        if(GetComponentInChildren<ParticleSystem>())
            GetComponentInChildren<ParticleSystem>().gameObject.SetActive(false);

        if(GetComponent<GolemBossController>())
            GetComponent<GolemBossController>().enabled = false;
        else if(GetComponent<HeartHoarderBossController>())
            GetComponent<HeartHoarderBossController>().enabled = false;
        else if(GetComponent<BloodKingBossController>())
            GetComponent<BloodKingBossController>().enabled = false;
        //state = GolemState.Dead;
    }

    public void Dead()
    {
        roomManager.CheckEnemyDead(this.gameObject);

        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("AttackCollider")) return;

        if (collision.TryGetComponent(out DamageDealer dealer))
        {
            TakeDamage(dealer.damage);
        }
    }

}
