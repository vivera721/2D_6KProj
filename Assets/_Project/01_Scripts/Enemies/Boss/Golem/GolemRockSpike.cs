using UnityEngine;

public class GolemRockSpike : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifeTime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDamage(int value)
    {
        damage = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null) return;

        playerHealth.TakeDamage(damage);
    }
}