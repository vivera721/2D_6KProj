using System.Collections;
using UnityEngine;

public class BloodKingBossController : BossBase
{
    BossHealth health;

    [Header("Hitboxes")]
    [SerializeField] private BoxCollider2D doubleSlashCollider;
    [SerializeField] private BoxCollider2D dodgeChargeSlashCollider;
    [SerializeField] private BoxCollider2D jumpSlamAttackCollider;
    [SerializeField] private BoxCollider2D stabAttackCollider;
    [SerializeField] private BoxCollider2D spinAttackCollider;

    [Header("WakeUp")]
    [SerializeField] private Transform bossFirstPoint;
    [SerializeField] private Transform wakeUpAppearPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Ranges")]
    [SerializeField] private float doubleSlashRange = 1.8f;
    [SerializeField] private float dodgeChargeRange = 4f;
    [SerializeField] private float jumpSlamRange = 5f;
    [SerializeField] private float stabSpinRange = 3.5f;

    [Header("Attack")]
    [SerializeField] private float attackInterval = 0.8f;

    [Header("DodgeChargeSlash")]
    [SerializeField] private float dodgeSpeed = 6f;
    [SerializeField] private float chargeSlashSpeed = 6f;
    [SerializeField] private float dodgeChargeDistance = 3f;
    [SerializeField] Transform DodgePoint;
    private bool isChargeSlashing;
    private float chargeSlashMovedDistance;
    [SerializeField] private float chargeSlashMaxDistance = 4f;

    [Header("Phase2")]
    [SerializeField] private float phase2HpRate = 0.5f;
    [SerializeField] private float phase2AttackInterval = 0.55f;
    [SerializeField] private float phase2MoveSpeed = 5.2f;
    [SerializeField] private float phase2ChargeDuration = 1.2f;
    private Coroutine phase2Coroutine;

    [Header("StabAndSpin")]
    [SerializeField] private float stabSpeed = 9f;
    [SerializeField] private float stabMaxDistance = 2.8f;

    private bool isStabbing;
    private float stabMovedDistance;

    private enum JumpSlamVariation
    {
        ShortLeap,
        LongLeap
    }
    [Header("JumpSlam Movement")]
    [SerializeField] private float shortLeapDistance = 5f;
    [SerializeField] private float longLeapDistance = 10f;
    [SerializeField] private float longLeapTriggerDistance = 7f;
    [SerializeField] private float jumpSlamHeight = 6f;
    [SerializeField] private float shortLeapDuration = 0.4f;
    [SerializeField] private float longLeapDuration = 0.6f;
    [Space(5)]
    [SerializeField] private float jumpSlamLandTolerance = 0.1f; // 착지 위치 허용 오차

    private bool isJumpSlamMoving;
    private float jumpSlamTimer;
    private float jumpSlamDuration;
    private float groundY;

    private Vector3 jumpStartPos;
    private Vector3 jumpLandPos;

    private JumpSlamVariation currentJumpSlamVariation;

    [Header("Attack Recovery")]
    [SerializeField] private float attackRecoveryTime = 0.6f;
    [SerializeField] private float phase2AttackRecoveryTime = 0.4f;
    private bool isRecovering;
    private float recoveryEndTime;

    //[Header("Dodge Back")]
    private bool isDodgingBack;
    private Vector3 dodgeTargetPos;
    private bool isPhase2;

    Vector3 appearTargetPos;

    private BloodKingState state = BloodKingState.StaticIdle;
    private BloodKingAttackType currentAttack = BloodKingAttackType.None;
    private BloodKingAttackType lastAttack = BloodKingAttackType.None;

    private bool isActing = false;
    private float nextAttackTime;
    private int facing = 1;


    protected override void Awake()
    {
        transform.position = bossFirstPoint.position;

        base.Awake();

        health = GetComponent<BossHealth>();

        DisableAllHitboxes();
    }
    public override void PlayWakeUp()
    {
        if (state != BloodKingState.StaticIdle) return;

        if (bossFirstPoint != null)
            transform.position = bossFirstPoint.position;

        if (wakeUpAppearPoint != null)
            appearTargetPos = wakeUpAppearPoint.position;

        state = BloodKingState.WakeUpVanish;
        SetMoveAnim(false);

        animator.SetTrigger("WakeUp");
    }
    public override void StartBattle()
    {
        if (IsBattleStarted) return;

        base.StartBattle();

        groundY = transform.position.y;

        state = BloodKingState.Idle;
        nextAttackTime = Time.time + attackInterval;

        SetMoveAnim(false);
    }

    private void Update()
    {
        if (IsDead) return;
        if (!IsBattleStarted) return;
        if (target == null) return;

        CheckPhase();
        UpdateSpecialMovement();

        if (isRecovering)
        {
            SetMoveAnim(false);

            if (Time.time > recoveryEndTime)
            {
                isRecovering = false;
                state = BloodKingState.Idle;
            }

            return;
        }

        switch (state)
        {
            case BloodKingState.StaticIdle:
                SetMoveAnim(false);
                break;

            case BloodKingState.WakeUp:
                SetMoveAnim(false);
                break;

            case BloodKingState.WakeUpVanish:
                SetMoveAnim(false);
                break;

            case BloodKingState.WakeUpAppear:
                SetMoveAnim(false);
                break;

            case BloodKingState.Idle:
                SetMoveAnim(false);
                HandleIdleOrMove();
                break;

            case BloodKingState.Move:
                SetMoveAnim(true);
                HandleIdleOrMove();
                LockFacingToTarget();
                break;

            case BloodKingState.Acting:
                SetMoveAnim(false);
                break;

            case BloodKingState.Vanish:
            case BloodKingState.Appear:
            case BloodKingState.Finisher:
            case BloodKingState.Dead:
                SetMoveAnim(false);
                break;
        }
    }

    private void UpdateSpecialMovement()
    {
        if (isDodgingBack)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                dodgeTargetPos,
                dodgeSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, dodgeTargetPos) <= 0.05f)
            {
                isDodgingBack = false;
            }
        }

        if (isChargeSlashing)
        {
            float moveAmount = chargeSlashSpeed * Time.deltaTime;

            transform.position += Vector3.right * facing * moveAmount;
            chargeSlashMovedDistance += moveAmount;

            if (chargeSlashMovedDistance >= chargeSlashMaxDistance)
            {
                isChargeSlashing = false;
            }
        }

        if (isStabbing)
        {
            float moveAmount = stabSpeed * Time.deltaTime;

            transform.position += Vector3.right * facing * moveAmount;
            stabMovedDistance += moveAmount;

            if (stabMovedDistance >= stabMaxDistance)
            {
                isStabbing = false;
            }
        }

        if (isJumpSlamMoving)
        {
            jumpSlamTimer += Time.deltaTime;
            float t = jumpSlamTimer / jumpSlamDuration;
            t = Mathf.Clamp01(t);

            float x = Mathf.Lerp(jumpStartPos.x, jumpLandPos.x, t);
            float y = groundY + jumpSlamHeight * 4f * t * (1f - t);

            transform.position = new Vector3(x, y, transform.position.z);

            if (t >= 1f)
            {
                transform.position = jumpLandPos;
                isJumpSlamMoving = false;
            }
        }

    }

    private void HandleIdleOrMove()
    {
        float distance = Mathf.Abs(target.position.x - transform.position.x);

        if (Time.time >= nextAttackTime)
        {
            BloodKingAttackType attack = DecideAttack(distance);
            StartAttack(attack);
            return;
        }

        if (distance > stopDistance)
        {
            state = BloodKingState.Move;
            MoveToTarget();
        }
        else
        {
            state = BloodKingState.Idle;
        }
    }

    private void StartAttack(BloodKingAttackType attack)
    {
        switch (attack)
        {
            case BloodKingAttackType.DoubleSlash:
                StartDoubleSlash();
                break;

            case BloodKingAttackType.StabAndSpin:
                StartStabAndSpin();
                break;

            case BloodKingAttackType.DodgeChargeSlash:
                StartDodgeChargeSlash();
                break;

            case BloodKingAttackType.JumpSlam:
                StartJumpSlam();
                break;
        }
    }

    private void MoveToTarget()
    {
        if (target == null) return;
        float dx = target.position.x - transform.position.x;
        UpdateFacing(dx);
        Vector3 moveDir = new Vector3(facing, 0f, 0f);
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private BloodKingAttackType DecideAttack(float distance)
    {
        //return BloodKingAttackType.StabAndSpin;
        // 테스트용


        BloodKingAttackType selected;

        if (distance <= doubleSlashRange)
        {
            // 근거리: 기본 베기 or 뒤잡기 방지 패턴
            selected = Random.value < 0.65f
                ? BloodKingAttackType.DoubleSlash
                : BloodKingAttackType.StabAndSpin;
        }
        else if (distance <= stabSpinRange)
        {
            // 중근거리: 찌르기+스핀 or 회피돌진
            selected = Random.value < 0.55f
                ? BloodKingAttackType.StabAndSpin
                : BloodKingAttackType.DodgeChargeSlash;
        }
        else if (distance <= dodgeChargeRange)
        {
            // 중거리: 시그니처 돌진베기
            selected = Random.value < 0.7f
                ? BloodKingAttackType.DodgeChargeSlash
                : BloodKingAttackType.JumpSlam;
        }
        else if (distance <= jumpSlamRange)
        {
            // 원거리: 점프 찍기 or 돌진 접근
            selected = Random.value < 0.6f
                ? BloodKingAttackType.JumpSlam
                : BloodKingAttackType.DodgeChargeSlash;
        }
        else
        {
            selected = BloodKingAttackType.JumpSlam;
        }

        selected = AvoidSameAttack(selected, distance);

        return selected;
    }

    private BloodKingAttackType AvoidSameAttack(BloodKingAttackType selected, float distance)
    {
        if (selected != lastAttack) return selected;

        if (selected == BloodKingAttackType.DoubleSlash)
        {
            if (distance <= stabSpinRange)
                return BloodKingAttackType.StabAndSpin;

            return BloodKingAttackType.DodgeChargeSlash;
        }

        if (selected == BloodKingAttackType.DodgeChargeSlash)
        {
            if (distance <= doubleSlashRange)
                return BloodKingAttackType.DoubleSlash;

            return BloodKingAttackType.JumpSlam;
        }

        if (selected == BloodKingAttackType.JumpSlam)
        {
            return BloodKingAttackType.DodgeChargeSlash;
        }

        if (selected == BloodKingAttackType.StabAndSpin)
        {
            return BloodKingAttackType.DoubleSlash;
        }

        return selected;
    }


    private void CheckPhase()
    {
        if (isPhase2) return;
        if (health == null) return;
        if (isActing) return;

        if (health.currentHealth <= health.maxHealth * phase2HpRate)
        {
            EnterPhase2();
        }
    }

    private void EnterPhase2()
    {
        isPhase2 = true;

        attackInterval = phase2AttackInterval;
        moveSpeed = phase2MoveSpeed;

        state = BloodKingState.Acting;
        isActing = true;

        // 차지 애니메이션이 준비되어 있다면 사용
        animator.SetTrigger("Charge");

        if(phase2Coroutine != null)
            StopCoroutine(phase2Coroutine);

        phase2Coroutine = StartCoroutine(CoPhase2ChargeEnd());

    }

    private IEnumerator CoPhase2ChargeEnd()
    {
        yield return new WaitForSeconds(phase2ChargeDuration);

        if (state == BloodKingState.Acting && isActing && currentAttack == BloodKingAttackType.None)
        {
            isActing = false;
            state = BloodKingState.Idle;
            nextAttackTime = Time.time + attackInterval;
        }
    }

    public void StartDoubleSlash()
    {
        if (isActing) return;

        LockFacingToTarget();

        isActing = true;
        currentAttack = BloodKingAttackType.DoubleSlash;
        state = BloodKingState.Acting;

        animator.SetTrigger("DoubleSlash");
    }

    public void StartDodgeChargeSlash()
    {
        if (isActing) return;

        LockFacingToTarget();

        isActing = true;
        currentAttack = BloodKingAttackType.DodgeChargeSlash;
        state = BloodKingState.Acting;

        animator.SetTrigger("DodgeChargeSlash");
    }

    public void StartStabAndSpin()
    {
        if (isActing) return;

        LockFacingToTarget();

        isActing = true;
        currentAttack = BloodKingAttackType.StabAndSpin;
        state = BloodKingState.Acting;

        animator.SetTrigger("StabAndSpin");
    }

    public void StartJumpSlam()
    {
        if (isActing) return;
        if(target == null) return;

        LockFacingToTarget();

        isActing = true;
        currentAttack = BloodKingAttackType.JumpSlam;
        state = BloodKingState.Acting;

        float distance = Mathf.Abs(target.position.x - transform.position.x);

        bool useLongLeap = distance >= longLeapTriggerDistance;

        float leapDistance = useLongLeap ? longLeapDistance : shortLeapDistance;
        jumpSlamDuration = useLongLeap ? longLeapDuration : shortLeapDuration;

        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        jumpStartPos = transform.position;

        float landX = jumpStartPos.x + facing * leapDistance;

        // 플레이어 지나치지 않게 착지 위치 조정
        if (facing == 1)
            landX = Mathf.Min(landX, target.position.x);
        else
            landX = Mathf.Max(landX, target.position.x);

        float peakX = (jumpStartPos.x + landX) / 2f;

        jumpLandPos = new Vector3(landX, groundY, transform.position.z);

        jumpSlamTimer = 0f;
        isJumpSlamMoving = false;

        animator.SetTrigger("JumpSlam");
    }

    private void LockFacingToTarget()
    {
        if (target == null) return;

        float dx = target.position.x - transform.position.x;
        UpdateFacing(dx);
    }

    private void UpdateFacing(float dx)
    {
        if (Mathf.Abs(dx) < 0.01f) return; // 거의 같은 위치면 방향 안 바꿈

        int dir = dx > 0f ? 1 : -1;
        if (dir == facing) return;

        facing = dir;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facing;
        transform.localScale = s;
    }

    private void SetMoveAnim(bool value)
    {
        if (animator != null)
            animator.SetBool("IsMoving", value);
    }

    //public void StartIntro(Transform player)
    //{
    //    if (state != BloodKingState.StaticIdle) return;

    //    target = player;

    //    // 확실하게 처음 위치에서 시작
    //    if (bossFirstPoint != null)
    //        transform.position = bossFirstPoint.position;

    //    appearTargetPos = wakeUpAppearPoint.position;

    //    state = BloodKingState.WakeUpVanish;
    //    SetMoveAnim(false);

    //    animator.SetTrigger("WakeUp");
    //}

    //private IEnumerator CoStartBattleAfterIntroDelay()
    //{
    //    state = BloodKingState.Idle;
    //    SetMoveAnim(false);

    //    yield return new WaitForSeconds(2f);

    //    StartBattle();
    //}

    private void DisableAllHitboxes()
    {
        if (doubleSlashCollider != null) doubleSlashCollider.enabled = false;
        if (dodgeChargeSlashCollider != null) dodgeChargeSlashCollider.enabled = false;
        if (jumpSlamAttackCollider != null) jumpSlamAttackCollider.enabled = false;
        if (stabAttackCollider != null) stabAttackCollider.enabled = false;
        if (spinAttackCollider != null) spinAttackCollider.enabled = false;
    }

    // ===== Animation Events =====

    public void AE_WakeUpEnd()
    {
        if (state != BloodKingState.WakeUp) return;

        state = BloodKingState.WakeUpVanish;
        appearTargetPos = wakeUpAppearPoint.position;
        animator.SetTrigger("WakeUp");
        //StartCoroutine(CoFirstPatternDelay());
    }

    public void AE_WakeUpVanishEnd()
    {
        if (state != BloodKingState.WakeUpVanish) return;

        transform.position = appearTargetPos;

        state = BloodKingState.WakeUpAppear;
        animator.SetTrigger("WakeUpAppear");

    }

    public void AE_VanishEnd()
    {
        if (state != BloodKingState.Vanish) return;

        transform.position = appearTargetPos;

        state = BloodKingState.Appear;
        animator.SetTrigger("Appear");

    }

    public void AE_WakeUpAppearEnd()
    {
        if (state != BloodKingState.WakeUpAppear) return;

        state = BloodKingState.Idle;
        SetMoveAnim(false);

        NotifyWakeUpFinished();
    }

    public void AE_AppearEnd()
    {
        if (state != BloodKingState.Appear) return;
        state = BloodKingState.Idle;
        nextAttackTime = Time.time + attackInterval;
    }

    public void AE_AttackEnd()
    {
        if (currentAttack == BloodKingAttackType.JumpSlam && isJumpSlamMoving)
            return;

        lastAttack = currentAttack;
        BloodKingAttackType finishedAttack = currentAttack;

        currentAttack = BloodKingAttackType.None;

        isActing = false;
        isDodgingBack = false;
        isChargeSlashing = false;
        isStabbing = false;
        isJumpSlamMoving = false;

        DisableAllHitboxes();

        if (TryComboAfterAttack(finishedAttack))
        {
            return;
        }

        StartAttackRecovery();
    }

    private void StartAttackRecovery()
    {
        isRecovering = true;

        float recoveryTime = isPhase2 ? phase2AttackRecoveryTime : attackRecoveryTime;
        recoveryEndTime = Time.time + recoveryTime;

        state = BloodKingState.Idle;
        nextAttackTime = Time.time + attackInterval;

        SetMoveAnim(false);
    }

    public void AE_DoubleSlashStart()
    {
        if(doubleSlashCollider != null)
            doubleSlashCollider.enabled = true;
    }

    public void AE_DoubleSlashEnd()
    {
        if(doubleSlashCollider != null)
            doubleSlashCollider.enabled = false;
    }

    public void AE_DodgeBackStart()
    {
        dodgeTargetPos = transform.position + Vector3.left * facing * dodgeChargeDistance;
        isDodgingBack = true;
    }

    public void AE_DodgeBackEnd()
    {
        isDodgingBack = false;
    }

    public void AE_ChargeSlashStart()
    {
        isChargeSlashing = true;
        chargeSlashMovedDistance = 0f;

        if (dodgeChargeSlashCollider != null)
            dodgeChargeSlashCollider.enabled = true;
    }

    public void AE_ChargeSlashEnd()
    {
        isChargeSlashing = false;

        if (dodgeChargeSlashCollider != null)
            dodgeChargeSlashCollider.enabled = false;
    }

    public void AE_StabStart()
    {
        isStabbing = true;
        stabMovedDistance = 0f;

        if (stabAttackCollider != null)
            stabAttackCollider.enabled = true;
    }

    public void AE_StabEnd()
    {
        isStabbing = false;

        if (stabAttackCollider != null)
            stabAttackCollider.enabled = false;
    }

    public void AE_SpinStart()
    {
        if (spinAttackCollider != null)
            spinAttackCollider.enabled = true;
    }

    public void AE_SpinEnd()
    {
        if (spinAttackCollider != null)
            spinAttackCollider.enabled = false;
    }
    public void AE_JumpRiseStart()
    {
        isJumpSlamMoving = true;
        jumpSlamTimer = 0f;
    }

    public void AE_JumpRiseEnd()
    {
        //isJumpRising = false;
    }
    public void AE_LockJumpSlamTarget()
    {
        //if (target == null) return;

        //lockedJumpSlamPos = target.position;

        //jumpLandPos = new Vector3(
        //    lockedJumpSlamPos.x,
        //    jumpStartPos.y,
        //    transform.position.z
        //);
    }
    public void AE_JumpFallStart()
    {
        //isJumpFalling = true;
    }
    public void AE_JumpFallEnd()
    {
        //isJumpFalling = false;
    }

    public void AE_JumpSlamHitboxStart()
    {
        if (jumpSlamAttackCollider != null)
            jumpSlamAttackCollider.enabled = true;
    }

    public void AE_JumpSlamHitboxEnd()
    {
        if (jumpSlamAttackCollider != null)
            jumpSlamAttackCollider.enabled = false;
    }

    public void AE_ChargeEnd()
    {
        if(phase2Coroutine != null)
        {
            StopCoroutine(phase2Coroutine);
            phase2Coroutine = null;
        }

        isActing = false;
        state = BloodKingState.Idle;
        nextAttackTime = Time.time + attackInterval;
    }
    private bool TryComboAfterAttack(BloodKingAttackType finishedAttack)
    {
        if (target == null) return false;

        float distance = Mathf.Abs(target.position.x - transform.position.x);

        float comboChance = isPhase2 ? 0.35f : 0.2f;

        if (finishedAttack == BloodKingAttackType.DoubleSlash)
        {
            // DoubleSlash 후 중거리면 뒤로 빠졌다가 돌진
            if (Random.value < comboChance && distance <= dodgeChargeRange)
            {
                StartDodgeChargeSlash();
                return true;
            }
        }

        if (finishedAttack == BloodKingAttackType.DodgeChargeSlash)
        {
            // 돌진 후 가까우면 바로 2연베기
            if (Random.value < comboChance && distance <= doubleSlashRange)
            {
                StartDoubleSlash();
                return true;
            }

            // 2페이즈에서는 돌진 후 스핀 연계 가능
            if (isPhase2 && Random.value < 0.25f && distance <= stabSpinRange)
            {
                StartStabAndSpin();
                return true;
            }
        }

        if (finishedAttack == BloodKingAttackType.JumpSlam)
        {
            // 점프 찍기 후 가까우면 스핀으로 압박
            if (Random.value < comboChance && distance <= stabSpinRange)
            {
                StartStabAndSpin();
                return true;
            }
        }

        if (finishedAttack == BloodKingAttackType.StabAndSpin)
        {
            // 스핀 후에는 너무 몰아치지 않도록 기본적으로 쉬게 둠
            return false;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, doubleSlashRange);
        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.position, dodgeChargeRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stabSpinRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, jumpSlamRange);
    }
}
