using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HeartHoarderBossController : BossBase
{
    private BossHealth health;
    private BoxCollider2D contactCollider;

    [Header("Hitboxes")]
    [SerializeField] private BoxCollider2D groundDashSlashCollider;
    [SerializeField] private BoxCollider2D stationarySpinSlashCollider;
    [SerializeField] private BoxCollider2D airSlamAttackCollider;
    [SerializeField] private PolygonCollider2D airSlamSword1_AttackCollider;
    [SerializeField] private PolygonCollider2D airSlamSword2_AttackCollider;

    [Header("State")]
    public HeartHoarderState state = HeartHoarderState.StaticIdle;
    public HeartHoarderAttackType currentAttack = HeartHoarderAttackType.None;
    private HeartHoarderAttackType lastAttack = HeartHoarderAttackType.None;

    [Header("Position")]
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform groundLeftPoint;
    [SerializeField] private Transform groundRightPoint;
    [SerializeField] private Transform airLeftPoint;
    [SerializeField] private Transform airRightPoint;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float lowHpMoveSpeedMultiplier = 1.15f;
    //[SerializeField] private float centerStopDistance = 0.2f;

    [Header("Dash Attack")]
    //[SerializeField] private float groundDashDistance = 8f;
    [SerializeField] private float groundDashSpeed = 8f;

    [Header("Air Dash Attack")]
    //[SerializeField] private float airDashDistance = 8f;
    [SerializeField] private float airDashSpeed = 9f;
    //[SerializeField] private float airDashHeight = 2.5f;

    [Header("Air Slam")]
    [SerializeField] private float airSlamHeight = 4f;
    [SerializeField] private float airSlamDownSpeed = 10f;

    [Header("Pattern Timing")]
    [SerializeField] private float firstPatternDelay = 1.5f;
    [SerializeField] private float patternInterval = 1.2f;
    [SerializeField] private float lowHpPatternInterval = 0.8f;

    [Header("Phase")]
    [Range(0f, 1f)]
    [SerializeField] private float phase2Threshold = 0.6f;
    [Range(0f, 1f)]
    [SerializeField] private float phase3Threshold = 0.3f;

    [Header("Damage")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private int groundDashSlashDamage = 18;
    [SerializeField] private int stationarySpinSlashDamage = 15;
    [SerializeField] private int airSlamDamage = 30;

    [Header("Death")]
    [SerializeField] private Transform deathPoint;
    [SerializeField] private bool useCenterDeathForAirAttack = true;

    private bool isDeathTeleporting;

    private bool isActing;
    private bool isResting;
    private bool isReturningToCenter;

    private bool isDashing;
    private bool isAirSlamming;

    private Vector3 dashTargetPos;
    private Vector3 appearTargetPos;

    protected override void Awake()
    {
        base.Awake();

        health = GetComponent<BossHealth>();
        contactCollider = GetComponent<BoxCollider2D>();
    }

    public override void PlayWakeUp()
    {
        state = HeartHoarderState.WakeUp;
        SetMoveAnim(false);

        animator.SetTrigger("WakeUp");
    }

    public override void StartBattle()
    {
        if (IsBattleStarted) return;

        base.StartBattle();

        state = HeartHoarderState.Idle;
        SetMoveAnim(false);

        StartCoroutine(CoFirstPatternDelay());
    }

    private void Update()
    {
        if (IsDead) return;
        if (!IsBattleStarted) return;
        if (target == null) return;

        HandleSpecialMotion();

        switch (state)
        {
            case HeartHoarderState.StaticIdle:
                SetMoveAnim(false);
                break;

            case HeartHoarderState.WakeUp:
                SetMoveAnim(false);
                break;

            case HeartHoarderState.Idle:
                SetMoveAnim(false);

                if (!isActing && !isResting)
                    DecideNextAction();
                break;

            case HeartHoarderState.Move:
                HandleMoveState();
                break;

            case HeartHoarderState.Acting:
                SetMoveAnim(false);
                break;

            case HeartHoarderState.Vanish:
                SetMoveAnim(false);
                break;

            case HeartHoarderState.Appear:
                SetMoveAnim(false);
                break;

            case HeartHoarderState.Dead:
                Die();
                currentAttack = HeartHoarderAttackType.None;
                SetMoveAnim(false);
                break;
        }
    }

    private void HandleMoveState()
    {
        if (isActing) return;

        state = HeartHoarderState.Idle;
        SetMoveAnim(false);
    }

    private void HandleSpecialMotion()
    {
        if (isDashing)
        {
            float dashSpeed = currentAttack == HeartHoarderAttackType.AirDashSlash ? airDashSpeed : groundDashSpeed;

            if (IsPhase3())
                dashSpeed *= 1.15f;

            transform.position = Vector3.MoveTowards(transform.position, dashTargetPos, dashSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, dashTargetPos) <= 0.01f)
            {
                isDashing = false;
                animator.SetTrigger("DashFinish");
            }
        }

        if (isAirSlamming)
        {
            //Vector3 slamTarget = new Vector3(transform.position.x, target.position.y, transform.position.z);
            Vector3 slamTarget = new Vector3(transform.position.x, centerPoint.position.y, transform.position.z);
            //Vector3 slamTarget = new Vector3(transform.position.x, 0.05f, transform.position.z);
            float slamSpeed = airSlamDownSpeed;

            if (IsPhase3())
                slamSpeed *= 1.15f;

            transform.position = Vector3.MoveTowards(transform.position, slamTarget, slamSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.y - slamTarget.y) <= 0.05f)
            {
                isAirSlamming = false;
            }
        }
    }

    private void DecideNextAction()
    {
        HeartHoarderAttackType nextAttack = ChooseNextAttack();

        switch (nextAttack)
        {
            case HeartHoarderAttackType.GroundDashSlash:
                StartGroundDashSlash();
                break;

            case HeartHoarderAttackType.AirDashSlash:
                StartAirDashSlash();
                break;

            case HeartHoarderAttackType.StationarySpinSlash:
                StartStationarySpinSlash();
                break;

            case HeartHoarderAttackType.AirSlam:
                StartAirSlam();
                break;
        }
    }

    private HeartHoarderAttackType ChooseNextAttack()
    {
        float dist = Mathf.Abs(target.position.x - transform.position.x);
        bool phase2 = IsPhase2();
        bool phase3 = IsPhase3();

        int groundDashWeight = 30;
        int airDashWeight = 0;
        int spinWeight = 30;
        int airSlamWeight = 0;

        if (dist <= 2.5f)
        {
            spinWeight += 25;
            groundDashWeight -= 10;
        }
        else
        {
            groundDashWeight += 10;
        }

        if (phase2)
        {
            airDashWeight = 20;
            airSlamWeight = 20;
        }

        if (phase3)
        {
            airDashWeight += 10;
            airSlamWeight += 15;
            groundDashWeight += 5;
        }

        switch (lastAttack)
        {
            case HeartHoarderAttackType.GroundDashSlash:
                groundDashWeight = 0;
                break;
            case HeartHoarderAttackType.AirDashSlash:
                airDashWeight = 0;
                break;
            case HeartHoarderAttackType.StationarySpinSlash:
                spinWeight = 0;
                break;
            case HeartHoarderAttackType.AirSlam:
                airSlamWeight = 0;
                break;
        }

        return WeightedPick(groundDashWeight, airDashWeight, spinWeight, airSlamWeight);
    }

    private HeartHoarderAttackType WeightedPick(int groundDash, int airDash, int spin, int airSlam)
    {
        List<(HeartHoarderAttackType type, int weight)> pool = new();

        if (groundDash > 0) pool.Add((HeartHoarderAttackType.GroundDashSlash, groundDash));
        if (airDash > 0) pool.Add((HeartHoarderAttackType.AirDashSlash, airDash));
        if (spin > 0) pool.Add((HeartHoarderAttackType.StationarySpinSlash, spin));
        if (airSlam > 0) pool.Add((HeartHoarderAttackType.AirSlam, airSlam));

        if (pool.Count == 0)
            return HeartHoarderAttackType.StationarySpinSlash;

        int total = 0;
        for (int i = 0; i < pool.Count; i++)
            total += pool[i].weight;

        int roll = Random.Range(0, total);
        int sum = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            sum += pool[i].weight;
            if (roll < sum)
                return pool[i].type;
        }

        return pool[pool.Count - 1].type;
    }

    private void StartGroundDashSlash()
    {
        isActing = true;
        state = HeartHoarderState.Vanish;
        currentAttack = HeartHoarderAttackType.GroundDashSlash;
        lastAttack = currentAttack;

        int rannum = Random.Range(0, 2);
        if (rannum > 0) 
        {
            appearTargetPos = groundLeftPoint.position;
            dashTargetPos = groundRightPoint.position;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            appearTargetPos = groundRightPoint.position;
            dashTargetPos = groundLeftPoint.position;
            transform.localScale = new Vector3(-1, 1, 1);
        }

        //bool startLeft = target.position.x >= centerPoint.position.x;
        //appearTargetPos = centerPoint.position + (startLeft ? Vector3.left : Vector3.right) * groundDashDistance * 0.5f;
        //dashTargetPos = centerPoint.position + (startLeft ? Vector3.right : Vector3.left) * groundDashDistance * 0.5f;

        animator.SetTrigger("Vanish");
    }

    private void StartAirDashSlash()
    {
        isActing = true;
        state = HeartHoarderState.Vanish;
        currentAttack = HeartHoarderAttackType.AirDashSlash;
        lastAttack = currentAttack;

        int rannum = Random.Range(0, 2);
        if (rannum > 0)
        {
            appearTargetPos = airLeftPoint.position;
            dashTargetPos = airRightPoint.position;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            appearTargetPos = airRightPoint.position;
            dashTargetPos = airLeftPoint.position;
            transform.localScale = new Vector3(-1, 1, 1);
        }

        //bool startLeft = target.position.x >= centerPoint.position.x;
        //appearTargetPos = centerPoint.position
        //                + (startLeft ? Vector3.left : Vector3.right) * airDashDistance * 0.5f
        //                + Vector3.up * airDashHeight;

        //dashTargetPos = centerPoint.position
        //              + (startLeft ? Vector3.right : Vector3.left) * airDashDistance * 0.5f
        //              + Vector3.up * airDashHeight;

        animator.SetTrigger("Vanish");
    }

    private void StartStationarySpinSlash()
    {
        isActing = true;
        state = HeartHoarderState.Acting;
        currentAttack = HeartHoarderAttackType.StationarySpinSlash;
        lastAttack = currentAttack;

        transform.position = centerPoint.position;
        SetMoveAnim(false);
        animator.SetTrigger("StationarySpinSlash");
    }

    private void StartAirSlam()
    {
        isActing = true;
        state = HeartHoarderState.Vanish;
        currentAttack = HeartHoarderAttackType.AirSlam;
        lastAttack = currentAttack;

        appearTargetPos = target.position + Vector3.up * airSlamHeight;
        animator.SetTrigger("Vanish");
    }

    public int GetDamageByAttackType(HeartHoarderAttackType attackType)
    {
        switch (attackType)
        {
            case HeartHoarderAttackType.GroundDashSlash:
                return groundDashSlashDamage;
            case HeartHoarderAttackType.AirDashSlash:
                return groundDashSlashDamage;
            case HeartHoarderAttackType.StationarySpinSlash:
                return stationarySpinSlashDamage;
            case HeartHoarderAttackType.AirSlam:
                return airSlamDamage;
        }

        return 0;
    }

    public int GetContactDamage()
    {
        return contactDamage;
    }

    private float GetCurrentMoveSpeed()
    {
        float speed = moveSpeed;

        if (IsPhase3())
            speed *= lowHpMoveSpeedMultiplier;

        return speed;
    }

    private bool IsPhase2()
    {
        if (health == null) return false;
        return (health.currentHealth / (float)health.maxHealth) <= phase2Threshold;
    }

    private bool IsPhase3()
    {
        if (health == null) return false;
        return (health.currentHealth / (float)health.maxHealth) <= phase3Threshold;
    }

    private void MoveToPosition(Vector3 targetPos, float speed)
    {
        SetMoveAnim(true);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    private IEnumerator CoFirstPatternDelay()
    {
        isResting = true;
        state = HeartHoarderState.Idle;
        SetMoveAnim(false);

        yield return new WaitForSeconds(firstPatternDelay);

        isResting = false;
    }

    private IEnumerator CoPatternInterval()
    {
        isResting = true;
        state = HeartHoarderState.Idle;
        SetMoveAnim(false);

        float wait = IsPhase3() ? lowHpPatternInterval : patternInterval;
        yield return new WaitForSeconds(wait);

        isResting = false;
    }

    private void SetMoveAnim(bool value)
    {
        if (animator != null)
            animator.SetBool("IsMoving", value);
    }

    // ===== Animation Events =====

    public void AE_WakeUpEnd()
    {
        if (state != HeartHoarderState.WakeUp) return;

        state = HeartHoarderState.Idle;
        SetMoveAnim(false);

        NotifyWakeUpFinished();
    }

    public void AE_ContactEnable()
    {
        if (contactCollider != null)
        {
            contactCollider.enabled = true;
        }
    }

    public void AE_ContactDisable()
    {
        if (contactCollider != null)
        {
            contactCollider.enabled = false;
        }
    }

    public void AE_VanishEnd()
    {
        if (state != HeartHoarderState.Vanish) return;

        if (isDeathTeleporting)
        {
            transform.position = deathPoint != null ? deathPoint.position : centerPoint.position;

            state = HeartHoarderState.Appear;
            animator.SetTrigger("Appear");
            return;
        }

        transform.position = appearTargetPos;
        state = HeartHoarderState.Appear;
        animator.SetTrigger("Appear");
    }

    public void AE_AppearEnd()
    {
        if (state != HeartHoarderState.Appear) return;

        if (isDeathTeleporting)
        {
            isDeathTeleporting = false;
            state = HeartHoarderState.Dead;

            SetMoveAnim(false);
            AE_AllHitboxOff();

            animator.SetTrigger("Die");
            return;
        }

        if (isReturningToCenter)
        {
            isReturningToCenter = false;
            state = HeartHoarderState.Idle;
            SetMoveAnim(false);
            StartCoroutine(CoPatternInterval());
            return;
        }

        state = HeartHoarderState.Acting;

        switch (currentAttack)
        {
            case HeartHoarderAttackType.GroundDashSlash:
                animator.SetTrigger("GroundDashSlash");
                break;

            case HeartHoarderAttackType.AirDashSlash:
                animator.SetTrigger("AirDashSlash");
                break;

            case HeartHoarderAttackType.AirSlam:
                animator.SetTrigger("AirSlam");
                break;
            case HeartHoarderAttackType.None:
                //case HeartHoarderAttackType.StationarySpinSlash:
                // 이 경우는 단순히 중앙으로 돌아오는 경우이므로 행동 없이 대기
                break;
        }
    }

    // 대쉬 시작 프레임에서 이벤트 호출
    public void AE_DashStart()
    {
        if (currentAttack == HeartHoarderAttackType.GroundDashSlash ||
            currentAttack == HeartHoarderAttackType.AirDashSlash)
        {
            isDashing = true;
        }
    }

    // 내려찍기 하강 시작 프레임에서 이벤트 호출
    public void AE_AirSlamStart()
    {
        if (currentAttack == HeartHoarderAttackType.AirSlam)
        {
            isAirSlamming = true;
        }
    }

    public void AE_AttackEnd()
    {
        isDashing = false;
        isAirSlamming = false;

        currentAttack = HeartHoarderAttackType.None;
        isActing = false;

        if (IsDead)
        {
            state = HeartHoarderState.Dead;
            return;
        }

        if(lastAttack == HeartHoarderAttackType.StationarySpinSlash)
        {
            state = HeartHoarderState.Idle;
            StartCoroutine(CoPatternInterval());
            return;
        }

        ReturnToCenter();
    }

    private void ReturnToCenter()
    {
        isReturningToCenter = true;
        //state = HeartHoarderState.Move;
        //SetMoveAnim(true);

        SetMoveAnim(false);
        appearTargetPos = centerPoint.position;
        state = HeartHoarderState.Vanish;
        animator.SetTrigger("Vanish");
    }

    public void AE_GroundDashSlashOn()
    {
        if (groundDashSlashCollider != null)
            groundDashSlashCollider.enabled = true;
    }

    public void AE_GroundDashSlashOff()
    {
        if (groundDashSlashCollider != null)
            groundDashSlashCollider.enabled = false;
    }

    public void AE_StationarySpinOn()
    {
        if (stationarySpinSlashCollider != null)
            stationarySpinSlashCollider.enabled = true;
    }

    public void AE_StationarySpinOff()
    {
        if (stationarySpinSlashCollider != null)
            stationarySpinSlashCollider.enabled = false;
    }

    public void AE_AirSlamOn()
    {
        if (airSlamAttackCollider != null)
            airSlamAttackCollider.enabled = true;
    }

    public void AE_AirSlamOff()
    {
        if (airSlamAttackCollider != null)
            airSlamAttackCollider.enabled = false;
    }

    public void AE_AirSlamSwordOn()
    {
        if (airSlamSword1_AttackCollider != null)
            airSlamSword1_AttackCollider.enabled = true;
        if (airSlamSword2_AttackCollider != null)
            airSlamSword2_AttackCollider.enabled = true;
    }

    public void AE_AirSlamSwordOff()
    {
        if (airSlamSword1_AttackCollider != null)
            airSlamSword1_AttackCollider.enabled = false;
        if (airSlamSword2_AttackCollider != null)
            airSlamSword2_AttackCollider.enabled = false;
    }

    public void AE_AllHitboxOff()
    {
        if (groundDashSlashCollider != null) groundDashSlashCollider.enabled = false;
        if (stationarySpinSlashCollider != null) stationarySpinSlashCollider.enabled = false;
        if (airSlamAttackCollider != null) airSlamAttackCollider.enabled = false;
        if (airSlamSword1_AttackCollider != null) airSlamSword1_AttackCollider.enabled = false;
        if (airSlamSword2_AttackCollider != null) airSlamSword2_AttackCollider.enabled = false;
    }

    public override void Die()
    {
        if (IsDead) return;

        bool shouldTeleportDeath = 
            useCenterDeathForAirAttack && 
            (currentAttack == HeartHoarderAttackType.AirDashSlash ||
            currentAttack == HeartHoarderAttackType.AirSlam || 
            isDashing || isAirSlamming);

        if( shouldTeleportDeath)
        {
            StartCoroutine(CoDeathTeleportToCenter());
            return;
        }


        DieImmediately();
    }

    IEnumerator CoDeathTeleportToCenter()
    {
        if (isDeathTeleporting) yield break;

        isDeathTeleporting = true;

        base.Die();

        StopAllCoroutines();

        isActing = false;
        isResting = false;
        isReturningToCenter = false;
        isDashing = false;
        isAirSlamming = false;

        currentAttack = HeartHoarderAttackType.None;

        SetMoveAnim(false);
        AE_AllHitboxOff();

        animator.ResetTrigger("GroundDashSlash");
        animator.ResetTrigger("AirDashSlash");
        animator.ResetTrigger("AirSlam");
        animator.ResetTrigger("StationarySpinSlash");
        animator.ResetTrigger("DashFinish");
        animator.ResetTrigger("Appear");
        animator.ResetTrigger("Die");

        appearTargetPos = deathPoint != null ? deathPoint.position : centerPoint.position;

        state = HeartHoarderState.Vanish;
        animator.SetTrigger("Vanish");

        yield return new WaitForSeconds(.3f);

        transform.position = appearTargetPos;

        state = HeartHoarderState.Appear;
        animator.ResetTrigger("Vanish");
        animator.SetTrigger("Appear");

        yield return new WaitForSeconds(.3f);

        state = HeartHoarderState.Dead;
        SetMoveAnim(false);

        animator.ResetTrigger("Appear");
        animator.SetTrigger("Die");
    }

    private void DieImmediately()
    {
        if (!IsDead) return;

        base.Die();

        StopAllCoroutines();

        isActing = false;
        isResting = false;
        isReturningToCenter = false;
        isDashing = false;
        isAirSlamming = false;
        isDeathTeleporting = false;

        currentAttack = HeartHoarderAttackType.None;
        state = HeartHoarderState.Dead;

        SetMoveAnim(false);
        AE_AllHitboxOff();

        animator.ResetTrigger("Vanish");
        animator.ResetTrigger("Appear");
        animator.ResetTrigger("GroundDashSlash");
        animator.ResetTrigger("AirDashSlash");
        animator.ResetTrigger("AirSlam");
        animator.ResetTrigger("StationarySpinSlash");
        animator.ResetTrigger("DashFinish");

        animator.ResetTrigger("Die");
    }

    private void OnDrawGizmosSelected()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(centerPoint.position, 0.3f);
        }
    }
}