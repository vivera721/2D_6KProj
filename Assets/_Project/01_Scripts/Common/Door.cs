using DG.Tweening;
using UnityEngine;

public class Door : MonoBehaviour
{
    BoxCollider2D doorCollider;
    DOTweenAnimation anim;
    Animator animator;

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        anim  = GetComponent<DOTweenAnimation>();
        animator = GetComponent<Animator>();
    }

    public void Open()
    {
        if (doorCollider != null)
            doorCollider.enabled = false;

        anim.DOPlayBackwards();

        //if (animator != null)
        //    animator.SetTrigger("Open");
    }

    public void Close()
    {
        if (doorCollider != null)
            doorCollider.enabled = true;

        anim.DOPlay();

        //if (animator != null)
        //    animator.SetTrigger("Close");
    }
}
