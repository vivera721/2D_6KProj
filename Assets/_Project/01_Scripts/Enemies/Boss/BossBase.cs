using System;
using UnityEngine;

public abstract class BossBase : MonoBehaviour
{
    public event Action OnWakeUpFinished;

    protected Animator animator;

    public Transform target { get; protected set; }

    public bool IsBattleStarted { get; private set; }
    public bool IsDead { get; protected set; }

    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public virtual void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public virtual void PlayWakeUp()
    {
        if (animator != null)
            animator.SetTrigger("WakeUp");
    }

    public virtual void StartBattle()
    {
        IsBattleStarted = true;
    }

    protected void NotifyWakeUpFinished()
    {
        OnWakeUpFinished?.Invoke();
    }

    public virtual void Die()
    {
        IsDead = true;
    }
}