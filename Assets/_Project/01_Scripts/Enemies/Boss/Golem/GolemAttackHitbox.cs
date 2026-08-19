using UnityEngine;

public class GolemAttackHitbox : MonoBehaviour
{
    [SerializeField] private GolemBossController owner;
    [SerializeField] private GolemAttackType attackType = GolemAttackType.None;

    [Header("Hit Delay")]
    [SerializeField] private float hitInterval = 0.2f;

    private float lastHitTime = -999f;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponentInParent<GolemBossController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (Time.time < lastHitTime + hitInterval) return;
        if (!other.CompareTag("Player")) return;
        if (owner == null) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null) return;

        int damage = owner.GetDamageByAttackType(attackType);
        playerHealth.TakeDamage(damage);

        lastHitTime = Time.time;
    }

    public void SetAttackType(GolemAttackType newType)
    {
        attackType = newType;
    }
}