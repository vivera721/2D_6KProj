using System.Collections;
using UnityEngine;

public enum UpgradeType
{
    HP,
    Stamina,
    Damage
}

public class Upgrade : MonoBehaviour
{
    public UpgradeType upgradeType;
    public int upgradeValue;

    Player player;
    PlayerHealth playerHealth;

    [SerializeField] private ParticleSystem Hp_Up_VFX;
    [SerializeField] private ParticleSystem St_Up_VFX;
    [SerializeField] private ParticleSystem Dmg_Up_VFX;

    private void Awake()
    {
        player = FindAnyObjectByType<Player>();
        playerHealth = FindAnyObjectByType<PlayerHealth>();
    }

    private void CheckType()
    {
        switch (upgradeType)
        {
            case UpgradeType.HP:
                playerHealth.IncreaseMaxHP(upgradeValue);
                Hp_Up_VFX.Play();
                break;
            case UpgradeType.Stamina:
                player.IncreaseMaxStamina(upgradeValue);
                St_Up_VFX.Play();
                break;
            case UpgradeType.Damage:
                player.attackDamage += (float)upgradeValue;
                Dmg_Up_VFX.Play();
                break;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CheckType();
            Destroy(gameObject);
        }
    }
}