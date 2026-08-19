using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GolemBossController : BossBase
{
    BossHealth health;

    [SerializeField] private Collider2D spinCollider;
    [SerializeField] private Collider2D beamCollider;
    [SerializeField] private Collider2D burstCollider;

    public GolemState state = GolemState.StaticIdle;
    public GolemAttackType currentAttack = GolemAttackType.None;

    private bool isActing;
    private int facing = 1;

    [Header("Move")]
    public float moveSpeed = 1.2f;
    public float stopDistance = 1.5f;

    [Header("Damage")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private int spinDamage = 15;
    [SerializeField] private int handSlamDamage = 20;
    [SerializeField] private int beamDamage = 18;
    [SerializeField] private int burstDamage = 30;

    [Header("Buff")]
    [SerializeField] private bool isBuffed = false;
    [SerializeField] private float buffMultiplier = 1.5f;
    [SerializeField] private DOTweenAnimation buffEffect;
    [SerializeField][Range(0f, 1f)] private float buffHpThreshold = 0.5f;
    [SerializeField] private float buffRecovery = 1.2f;

    private bool hasUsedBuff = false;

    [Header("Attack Range")]
    public float spinRange = 2.5f;
    public float slamMinRange = 2.2f;
    public float slamRange = 4f;
    public float beamMinRange = 4.2f;
    public float beamRange = 7f;
    public float burstRange = 4f; 
    
    [Header("Spin")]
    [SerializeField] private float spinDuration = 2.0f;
    private bool isSpinFinish = false;
    [SerializeField] private ParticleSystem spinVFX;

    private Coroutine spinRoutine;

    [Header("Burst")]
    [Range(0f,1f)] public float burstHpThreshold = 0.4f;
    
    [Header("Attack Cooldowns")]
    [SerializeField] private float burstCooldown = 10f;
    [SerializeField] private float handSlamCooldown = 3f;
    [SerializeField] private float beamCooldown = 5f;
    [SerializeField] private float spinCooldown = 4f;

    [Header("Hand Slam Rock Attack")]
    [SerializeField] private GameObject rockSpikePrefab;
    [SerializeField] private Vector3 rockSpawnOffset;

    [Header("Heavy Feel")]
    [SerializeField] private float actionInterval = 1.0f;
    [SerializeField] private float spinRecovery = 0.8f;
    [SerializeField] private float handSlamRecovery = 1.2f;
    [SerializeField] private float beamRecovery = 1.0f;
    [SerializeField] private float burstRecovery = 1.5f;

    private float nextActionTime;

    private float lastBurstTime = -999f;
    private float lastHandSlamTime = -999f;
    private float lastBeamTime = -999f;
    private float lastSpinTime = -999f;

    [Header("Camera Shake")]
    [SerializeField] private CameraShake cameraShake;

    protected override void Awake()
    {
        base.Awake();

        buffEffect = GetComponentInChildren<DOTweenAnimation>();
        //buffEffect.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        health = GetComponent<BossHealth>();
        spinVFX.Stop();
    }
    public override void PlayWakeUp()
    {
        state = GolemState.WakeUp;
        SetMoveAnim(false);

        animator.SetTrigger("WakeUp");
    }

    public override void StartBattle()
    {
        if (IsBattleStarted) return;

        base.StartBattle();

        state = GolemState.Idle;
        SetMoveAnim(false);
    }


    private void Update()
    {
        if (!IsBattleStarted) return;

        //Debug.Log($"state={state}, target={(target != null ? target.name : "NULL")}");

        //Debug.Log("state is " + state);

        if (IsDead) return;
        if(target == null) return;

        switch(state)
        {
            case GolemState.StaticIdle:
                SetMoveAnim(false);
                break;
            case GolemState.WakeUp:
                SetMoveAnim(false);
                // WakeUp Animation Play
                break;
            case GolemState.Idle:
                SetMoveAnim(false);
                if (!isActing && Time.time >= nextActionTime)
                {
                    DecideNextAction();
                }
                break;
            case GolemState.Move:
                if (!isActing)
                {
                    if (TryStartAttackByCondition())
                        return;

                    MoveToTarget();
                }
                break;
            case GolemState.Attack:
                SetMoveAnim(false);
                // 공격 중엔 AE_AttackEnd 기다림
                break;

            case GolemState.Dead:
                SetMoveAnim(false);
                break;
        }
    }

    public int GetDamageByAttackType(GolemAttackType attackType)
    {
        int baseDamage = 0;

        switch (attackType)
        {
            case GolemAttackType.Spin:
                baseDamage = spinDamage;
                break;

            case GolemAttackType.HandSlam:
                baseDamage = handSlamDamage;
                break;

            case GolemAttackType.Beam:
                baseDamage = beamDamage;
                break;

            case GolemAttackType.Burst:
                baseDamage = burstDamage;
                break;

            default:
                baseDamage = 0;
                break;
        }

        return GetFinalDamage(baseDamage);
    }

    public int GetFinalDamage(int baseDamage)
    {
        if (!isBuffed)
            return baseDamage;

        return Mathf.RoundToInt(baseDamage * buffMultiplier);
    }

    public int GetContactDamage()
    {
        return GetFinalDamage(contactDamage);
    }

    public void ApplyBuff(float multiplier)
    {
        isBuffed = true;
        buffMultiplier = multiplier;
    }

    public void ClearBuff()
    {
        isBuffed = false;
    }

    private void DecideNextAction()
    {
        if(TryStartAttackByCondition()) return;

        float dx = target.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        // 이미 충분히 가까우면 Move로 가지 말고 Idle에서 대기
        if (dist <= stopDistance)
        {
            state = GolemState.Idle;
            SetMoveAnim(false);
            nextActionTime = Time.time + 0.2f;
            return;
        }

        state = GolemState.Move;
        SetMoveAnim(true);
    }

    private bool TryStartAttackByCondition()
    {
        UpdateFacingToTarget();

        if (CanUseBuff())
        {
            StartAttack(GolemAttackType.Buff);
            return true;
        }

        if (CanUseSpin() && IsTargetInSpinRange())
        {
            StartAttack(GolemAttackType.Spin);
            return true;
        }

        if (CanUseBurst() && IsTargetInFrontRange(burstRange))
        {
            StartAttack(GolemAttackType.Burst);
            return true;
        }

        if (CanUseHandSlam() && IsTargetInFrontRange(slamMinRange, slamRange))
        {
            StartAttack(GolemAttackType.HandSlam);
            return true;
        }

        if (CanUseBeam() && IsTargetInFrontRange(beamMinRange, beamRange))
        {
            StartAttack(GolemAttackType.Beam);
            return true;
        }

        return false;
    }

    private bool CanUseBuff()
    {
        if (health == null) return false;
        if (hasUsedBuff) return false;
        if (isBuffed) return false;

        float hpRatio = health.currentHealth / (float)health.maxHealth;

        return hpRatio <= buffHpThreshold;
    }

    private bool CanUseSpin()
    {
        return Time.time >= lastSpinTime + spinCooldown;
    }

    private bool CanUseHandSlam()
    {
        return Time.time >= lastHandSlamTime + handSlamCooldown;
    }

    private bool CanUseBeam()
    {
        return Time.time >= lastBeamTime + beamCooldown;
    }

    private bool CanUseBurst()
    {
        if (health == null) return false;

        float hpRatio = health.currentHealth / (float)health.maxHealth;
        if (hpRatio > burstHpThreshold) return false;

        return Time.time >= lastBurstTime + burstCooldown;
    }

    private bool IsTargetInFrontRange(float minRange, float maxRange)
    {
        if (target == null) return false;

        float dx = target.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        if (dist < minRange) return false;
        if (dist > maxRange) return false;
        if (!IsTargetInFront()) return false;

        return true;
    }

    private void MoveToTarget()
    {
        float dx = target.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        UpdateFacing(dx);
        SetMoveAnim(true);

        if (dist <= stopDistance) 
        {
            state = GolemState.Idle;
            SetMoveAnim(false);
            return;
        }

        Vector3 pos = transform.position;
        pos.x += Mathf.Sign(dx) * moveSpeed * Time.deltaTime;
        transform.position = pos;
    }
    private void SetMoveAnim(bool value)
    {
        if (animator != null)
            animator.SetBool("IsMoving", value);
    }

    private bool IsTargetInFront()
    {
        float dx = target.position.x - transform.position.x;
        return dx * facing > 0f;
    }

    private bool IsTargetInFrontRange(float range)
    {
        float dx = target.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        if (dist > range) return false;
        if (!IsTargetInFront()) return false;

        return true;
    }

    private bool IsTargetInSpinRange()
    {
        if (target == null) return false;

        float dx = target.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        return dist <= spinRange;
    }

    private void UpdateFacingToTarget()
    {
        float dx = target.position.x - transform.position.x;
        UpdateFacing(dx);
    }

    private void UpdateFacing(float dx)
    {
        if(Mathf.Abs(dx) < 0.01f) return; // 거의 같은 위치면 방향 안 바꿈

        int dir = dx > 0f ? 1 : -1;
        if(dir == facing) return;

        facing = dir;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        transform.localScale = s;
    }

    private void StartAttack(GolemAttackType attackType)
    {
        isActing = true;
        state = GolemState.Attack;
        currentAttack = attackType;

        SetMoveAnim(false);

        if (attackType == GolemAttackType.Burst)
        {
            lastBurstTime = Time.time;
        }

        switch (attackType)
        {
            case GolemAttackType.Spin:
                isSpinFinish = false;
                animator.SetBool("SpinFinish", isSpinFinish);
                lastSpinTime = Time.time;
                animator.SetTrigger("Spin");
                spinVFX.Play();

                if (spinRoutine != null)
                    StopCoroutine(spinRoutine);

                spinRoutine = StartCoroutine(SpinAttackRoutine());
                break;
            case GolemAttackType.HandSlam:
                lastHandSlamTime = Time.time;
                animator.SetTrigger("HandSlam");
                break;
            case GolemAttackType.Beam:
                lastBeamTime = Time.time;
                animator.SetTrigger("Beam");
                break;
            case GolemAttackType.Burst:
                lastBurstTime = Time.time;
                animator.SetTrigger("Burst");
                break;
            case GolemAttackType.Buff:
                hasUsedBuff = true;
                animator.SetTrigger("Buff");
                break;
        }
    }
    private IEnumerator SpinAttackRoutine()
    {
        if (spinCollider != null)
            spinCollider.enabled = true;

        yield return new WaitForSeconds(spinDuration);

        if (spinCollider != null)
            spinCollider.enabled = false;

        spinRoutine = null;
        isSpinFinish = true;

        animator.SetBool("SpinFinish", isSpinFinish);

        EndAttack();
    }

    private void EndAttack()
    {
        GolemAttackType endedAttack = currentAttack;
        float recovery = GetRecoveryTime(currentAttack);

        currentAttack = GolemAttackType.None;
        isActing = false;

        if (IsDead)
        {
            state = GolemState.Dead;
            return;
        }

        if (endedAttack == GolemAttackType.Buff)
        {
            nextActionTime = Time.time + 0.5f;
        }
        else
        {
            nextActionTime = Time.time + recovery;
        }

        state = GolemState.Idle;
        SetMoveAnim(false);
    }

    public void SpawnRockSpikeAtPlayer()
    {
        if (target == null || rockSpikePrefab == null) return;

        Vector3 spawnPos = target.position + rockSpawnOffset;
        GameObject obj = Instantiate(rockSpikePrefab, spawnPos, Quaternion.identity);

        GolemRockSpike spike = obj.GetComponent<GolemRockSpike>();
        spike.GetComponent<Animator>().SetTrigger("Appear");
        if (spike != null)
            spike.SetDamage(GetDamageByAttackType(GolemAttackType.HandSlam));
    }

    public void AE_WakeUpEnd()
    {
        if (state != GolemState.WakeUp) return;

        state = GolemState.Idle;
        SetMoveAnim(false);

        NotifyWakeUpFinished();
    }

    public void AE_AttackEnd()
    {
        if (currentAttack == GolemAttackType.Spin)
            return;

        EndAttack();
    }

    private float GetRecoveryTime(GolemAttackType attackType)
    {
        switch (attackType)
        {
            case GolemAttackType.Spin:
                return spinRecovery;

            case GolemAttackType.HandSlam:
                return handSlamRecovery;

            case GolemAttackType.Beam:
                return beamRecovery;

            case GolemAttackType.Burst:
                return burstRecovery;
            case GolemAttackType.Buff:
                return buffRecovery;

            default:
                return actionInterval;
        }
    }

    public void AE_ApplyBuff_15()
    {
        ApplyBuff(1.5f);
    }

    public void AE_ApplyBuff_20()
    {
        ApplyBuff(2.0f);
    }

    public void AE_SwingOn()
    {
        if (spinCollider != null) spinCollider.enabled = true;
        animator.speed = 1.5f; // 스핀 애니메이션 속도 증가
    }

    public void AE_SwingOff()
    {
        if (spinCollider != null) spinCollider.enabled = false;
        animator.speed = 1.0f; // 스핀 애니메이션 속도 원래대로
        spinVFX.Stop();
        //spinVFX.SetActive(false);
    }

    public void AE_BeamOn()
    {
        if (beamCollider != null) beamCollider.enabled = true;
    }

    public void AE_BeamOff()
    {
        if (beamCollider != null) beamCollider.enabled = false;
    }

    public void AE_BurstOn()
    {
        if (burstCollider != null) burstCollider.enabled = true;
    }

    public void AE_BurstOff()
    {
        if (burstCollider != null) burstCollider.enabled = false;
    }

    public void AE_AllAttackCollidersOff()
    {
        if (spinCollider == null || beamCollider == null || burstCollider == null) return;

        spinCollider.enabled = false;
        beamCollider.enabled = false;
        burstCollider.enabled = false;

    }
    public void AE_HeavyImpact()
    {
        // 카메라 쉐이크
        cameraShake.ShakeStrong();
        // 히트스톱
        // 먼지 이펙트
        // 바닥 파편
        // or
        // FeelFeedbacks?.PlayFeedbacks();
    }

    public void AE_BuffEffect()
    {
        if (buffEffect != null)
            buffEffect.DOPlay();
    }

    public override void Die()
    {
        base.Die();

        isActing = false;
        currentAttack = GolemAttackType.None;
        state = GolemState.Dead;
        spinVFX.gameObject.SetActive(false);

        SetMoveAnim(false);

        animator.SetTrigger("Die");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spinRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, slamRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, beamRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, burstRange);
    }
}
