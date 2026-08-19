using UnityEngine;

public class BossContactDamage : MonoBehaviour
{
    private GolemBossController owner;
    [SerializeField] private float hitInterval = 0.5f;

    private float lastHitTime = -999f;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponent<GolemBossController>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < lastHitTime + hitInterval) return;
        if (!other.CompareTag("Player")) return;
        if (owner == null) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null) return;

        int damage = owner.GetContactDamage();
        playerHealth.TakeDamage(damage);
        lastHitTime = Time.time;
    }
}