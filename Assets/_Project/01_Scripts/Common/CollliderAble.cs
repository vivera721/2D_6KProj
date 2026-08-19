using UnityEngine;

public class CollliderAble : MonoBehaviour
{
    private BoxCollider2D col;
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }

    public void EnableCollider()
    {
        if (col != null)
            col.enabled = true;
    }
    public void DisableCollider()
    {
        if (col != null)
            col.enabled = false;
    }
}
