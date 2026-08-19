using UnityEngine;

public enum DamageOwner
{
    Player,
    Enemy
}

public class DamageDealer : MonoBehaviour
{
    [Header("Owner")]
    public DamageOwner ownerType;
    public Transform owner;

    [Header("Damage")]
    public int damage = 1;
    [SerializeField] private bool usePlayerAttackDamage = false;

    [Header("KnockBack")]
    public float knockbackForce = 6f;
    public float knockbackUpForce = 2f;
    public float hitStunTime = 0.1f;


    private void Awake()
    {
        if(owner == null)
            owner = transform.parent;

        if (usePlayerAttackDamage)
        {
            Player player = GetComponentInParent<Player>();
            if(player != null )
                damage = (int)player.attackDamage;
        }
    }

}
