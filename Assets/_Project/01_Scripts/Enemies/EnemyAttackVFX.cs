using UnityEngine;

public class EnemyAttackVFX : MonoBehaviour
{
    [SerializeField]private Animator animator;

    public void PlayAttackVFX()
    {
        if (animator == null) return;
        animator.ResetTrigger("VFX");   // 안전빵(연속 호출 시)
        animator.SetTrigger("VFX");
    }
}