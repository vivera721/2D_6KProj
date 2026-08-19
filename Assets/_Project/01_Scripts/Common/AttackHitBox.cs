using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
            if(player.isLowerAttack)
            {
                player.BounceFromDownAttack();
            }
        }
    }
}
