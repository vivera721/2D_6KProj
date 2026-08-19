using UnityEngine;

public class PlayerRuntimeData : MonoBehaviour
{
    public static PlayerRuntimeData Instance { get; private set; }

    public bool HasData { get; private set; }

    public int maxHP;
    public int currentHP;
    public int maxStamina;
    public int currentStamina;
    public float attackDamage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveFromPlayer(Player player, PlayerHealth health)
    {
        if (player == null || health == null) return;

        maxHP = health.MaxHP;
        currentHP = health.CurrentHP;

        maxStamina = player.MaxStaminaInt;
        currentStamina = player.CurrentStaminaInt;

        attackDamage = player.attackDamage;

        HasData = true;
    }

    public void Clear()
    {
        HasData = false;

        maxHP = 0;
        currentHP = 0;
        maxStamina = 0;
        currentStamina = 0;
        attackDamage = 0f;
    }
}