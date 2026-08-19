using UnityEngine;

public class BloodKingAttackHitbox : MonoBehaviour
{
    [SerializeField] private BloodKingBossController owner;
    [SerializeField] private BloodKingAttackType attackType = BloodKingAttackType.None;

    [Header("Hit Delay")]
    [SerializeField] private float hitInterval = 0.2f;

    private float lastHitTime = -999f;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponentInParent<BloodKingBossController>();
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

        //int damage = owner.GetDamageByAttackType(attackType);
        //playerHealth.TakeDamage(damage);

        lastHitTime = Time.time;
    }

    public void SetAttackType(BloodKingAttackType newType)
    {
        attackType = newType;
    }
}