using UnityEngine;

public class RestPlace : MonoBehaviour
{
    PlayerHealth playerHealth;

    [SerializeField] private Transform savePoint;
    Animator anim;

    private void Awake()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        anim = GetComponent<Animator>();
    }
    public void StartRest(Player player)
    {
        player.PlayRestAnimation();
        anim.SetTrigger("Save");
    }

    public void ApplyRest(Player player)
    {
        if(playerHealth != null) 
            playerHealth.Heal(playerHealth.MaxHP);

        Vector3 savePosition = savePoint != null ? savePoint.position : player.transform.position;

        SaveManager.Instance.Save(savePosition);

        Debug.Log("Rest and Save");
    }

}
