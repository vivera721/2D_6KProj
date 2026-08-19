using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    BoxCollider2D m_BoxCollider;
    RoomManager roomManager;

    private void Awake()
    {
        roomManager = GetComponentInParent<RoomManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            roomManager.StartRoom();
        }
    }
}
