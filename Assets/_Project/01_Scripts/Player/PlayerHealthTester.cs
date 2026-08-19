using UnityEngine;

public class PlayerHealthTester : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    private void Update()
    {
        if (playerHealth == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerHealth.TakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerHealth.Heal(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            playerHealth.IncreaseMaxHP(1, true);
        }
    }
}