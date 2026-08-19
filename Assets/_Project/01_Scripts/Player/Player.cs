using System.ComponentModel;
using UnityEngine;

public enum VFX_Type
{
    Slash,
    UpperSlash,
    LowerSlash,
    RangeSlash,
    DodgeSlash,
    Dash,
    Appear,
    Disappear,
    Parry
}

public class Player : MonoBehaviour
{
    [Header("Stamina")]
    public int maxStamina = 10;
    [SerializeField] private int currentStamina = 10;
    [SerializeField] private int staminaRegenRate = 1;

    [SerializeField] private int dodgeCost = 4;
    [SerializeField] private float regenDelayAfterUse = 0.5f;
    [SerializeField] private PlayerStaminaUI staminaUI;

    private float lastStaminaUseTime;
    private float staminaRegenTimer;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaNormalized => (float)currentStamina / maxStamina;

    [Header("Move")] 
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 12f; 
    private bool jumpRequested;

    [Header("Better Jump")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("DownAttack Bounce")]
    [SerializeField] private float downAttackBounceForce = 8f;
    [SerializeField] private float downAttackBounceCooldown = 0.1f;
    private float lastDownAttackBounceTime;
    public bool isLowerAttack = false;

    [Header("Ground & Wall Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private Collider2D attack1Collider;        // attack1 공격용 히트박스
    [SerializeField] private Collider2D attack2Collider;        // attack2 공격용 히트박스
    [SerializeField] private Collider2D attack3Collider;        // attack3 공격용 히트박스
    [SerializeField] private Collider2D lowerAttackCollider;   // 하단 공격용 히트박스
    [SerializeField] private Collider2D upperAttackCollider;   // 상단 공격용 히트박스
    [Space(5)]
    public float attackDamage = 10f;
    [Space(5)]
    public LayerMask enemyLayers;
    public float attackRate = 2f; // 초당 공격 횟수 (2면 0.5초 쿨)
    [SerializeField] private float comboResetTime = 0.8f;
    private float comboExpireTime;

    [Header("Attack Movement")]
    [SerializeField] private float airAttackMoveMultiplier = 0.4f;

    [Header("Dodge")]
    [SerializeField] private float dodgeSpeed = 12f;
    [SerializeField] private float dodgeDuration = 0.22f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    [SerializeField] private float dodgeInvincible = 0.12f;
    [SerializeField] private bool canDodgeCancelAttack = true;

    private bool isDodging;
    private float dodgeTimer;
    private float nextDodgeTime;
    private int dodgeDir;

    /*
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;

    private bool isDashing;
    private float dashTimer;
    private float nextDashTime;
    private int dashDir;
    */

    private int comboIndex = 0;
    private float lastAttackTime;

    [Header("VFX")]
    [SerializeField] private PlayerVFX vfx;
    [SerializeField] private bool VFXCheck = false; 

    private Rigidbody2D rb;
    [HideInInspector]public Animator anim;

    private bool canControl = true;
    private Vector2 moveInput;

    float inputX;
    bool IsGround;
    bool isFacingRight = true;
    float nextAttackTime = 0f; 

    private bool isAirAttack;

    PlayerHealth health;

    private bool wasFalling = false;
    
    private bool isAttacking;
    private bool lockFacing;

    private RestPlace currentRestPlace;
    [SerializeField] private ParticleSystem RestVFX;

    [SerializeField] private PlayerAudio playerAudio;

    public int MaxStaminaInt => maxStamina;
    public int CurrentStaminaInt => currentStamina;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<PlayerHealth>();
        playerAudio = GetComponent<PlayerAudio>();
    }

    private void Start()
    {
        RestVFX.gameObject.SetActive(false);
        TryApplyRuntimeData();

        TryLoadSavedStatus();
        TryLoadSavedPosition();
        RefreshStaminaUI();
    }
    private void TryApplyRuntimeData()
    {
        if (PlayerRuntimeData.Instance == null) return;
        if (!PlayerRuntimeData.Instance.HasData) return;

        ApplyRuntimeStats(
            PlayerRuntimeData.Instance.maxStamina,
            PlayerRuntimeData.Instance.currentStamina,
            PlayerRuntimeData.Instance.attackDamage
        );
    }
    public void ApplyRuntimeStats(int savedMaxStamina, int savedCurrentStamina, float savedDamage)
    {
        if (savedMaxStamina > 0)
            maxStamina = savedMaxStamina;

        currentStamina = Mathf.Clamp(savedCurrentStamina, 0, maxStamina);

        if (savedDamage > 0f)
            attackDamage = savedDamage;

        RefreshStaminaUI();
    }
    private void TryLoadSavedStatus()
    {
        if (SaveManager.Instance == null) return;
        if (!SaveManager.Instance.IsContinueMode) return;
        if (!SaveManager.Instance.HasSaveData()) return;

        int savedMaxStamina = SaveManager.Instance.LoadStamina();
        float savedDMG = SaveManager.Instance.LoadDMG();

        if (savedMaxStamina > 0)
        {
            maxStamina = savedMaxStamina;
            currentStamina = maxStamina;
        }

        if (savedDMG > 0f)
        {
            attackDamage = savedDMG;
        }

        RefreshStaminaUI();
    }

    private void TryLoadSavedPosition()
    {
        if (SaveManager.Instance == null) return;
        if(!SaveManager.Instance.IsContinueMode) return;
        if(!SaveManager.Instance.HasSaveData()) return;

        Vector3 savedPosition = SaveManager.Instance.Load();
        transform.position = savedPosition;

        anim.SetTrigger("Rest");

        SaveManager.Instance.SetContinueMode(false); // 위치 불러온 후에는 계속 모드 해제
    }

    public void SetControlEnabled(bool enabled)
    {
        canControl = enabled;

        if (!canControl)
        {
            moveInput = Vector2.zero;
        }
    }

    public void StopImmediately()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Animator가 있다면 이동 애니메이션도 꺼주기
        // animator.SetBool("IsMoving", false);
    }


    void Update()
    {
        if (!canControl)
        {
            return;
        }

        RegenerateStamina();

        // --- input ---
        inputX = Input.GetAxisRaw("Horizontal");

        if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            TryRest();
        }

        // --- Jump Input ---
        if (Input.GetKeyDown(KeyCode.Z))
        {
            jumpRequested = true;
        }

        // --- Dodge Input ---
        if (Input.GetKeyDown(KeyCode.C))
        {
            TryDodge();
        }

        // --- Attack Input ---
        if (Time.time >= nextAttackTime && Input.GetKeyDown(KeyCode.X))
        {
            TryAttack();
        }

        Flip();

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(inputX), 0.05f, Time.deltaTime);
        }

        /*
        if (IsGround && rb.linearVelocity.x == 0)
        {
            bool lookUp = Input.GetKey(KeyCode.UpArrow);
            bool lookDown = Input.GetKey(KeyCode.DownArrow);

            if (lookUp)
                lookDown = false;

            anim.SetBool("LookUp", lookUp);
            anim.SetBool("LookDown", lookDown);
        }
        */
    }

    void FixedUpdate()
    {
        if (!canControl)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            return;
        }

        if (health != null && health.IsKnockback)
            return;

        bool previousGrounded = IsGround;

        // --- Ground Check ---
        IsGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- Landing Check ---
        if (!previousGrounded && IsGround)
        {
            OnLanded();
        }

        // --- Dodge ---
        if(isDodging)
        {
            DodgeMove();

            if (anim != null)
            {
                anim.SetBool("IsGround", IsGround);
                anim.SetFloat("YSpeed", rb.linearVelocity.y);
            }

            return;
        }

        // --- Jump ---
        if (jumpRequested)
        {
            if (IsGround && !isAttacking && !isDodging)
            {
                Jump();
            }

            jumpRequested = false;
        }

        // --- Movement ---
        if (isAttacking)
        {
            if (IsGround)
            {
                // 땅 공격 중 x 이동 정지
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            else
            {
                // 공중 공격 중 약한 조작 허용
                rb.linearVelocity = new Vector2(inputX * moveSpeed * airAttackMoveMultiplier, rb.linearVelocity.y);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);
        }

        ApplyBetterJump();

        // --- Animator ---
        if (anim != null)
        {
            anim.SetBool("IsGround", IsGround);
            anim.SetFloat("YSpeed", rb.linearVelocity.y);
        }
    }

    private void TryRest()
    {
        if (currentRestPlace == null) return;

        currentRestPlace.StartRest(this);

        canControl = false;
    }

    public void PlayRestAnimation()
    {
        anim.SetTrigger("Rest");
    }

    void TryAttack()
    {
        if (isAttacking) return;
        if (isDodging) return;

        bool pressUp = Input.GetKey(KeyCode.UpArrow);
        bool pressDown = Input.GetKey(KeyCode.DownArrow);

        if (!IsGround && pressDown)
        {
            AirSwingDown();
        }
        // 위 공격: 땅이면 UpwardsSwing, 공중이면 AirSwingUp
        else if (pressUp)
        {
            UpperAttack();
        }
        // 공중 횡베기
        else if (!IsGround)
        {
            AirSwingSide();
        }
        // 땅 기본 콤보
        else
        {
            GroundComboAttack();
        }

        nextAttackTime = Time.time + 1f / attackRate;
    }

    public void BounceFromDownAttack()
    {
        if(Time.time < lastDownAttackBounceTime + downAttackBounceCooldown)
            return;

        lastDownAttackBounceTime = Time.time;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, downAttackBounceForce);

        if(anim != null) {
            anim.SetBool("IsGround", false);
            anim.SetFloat("YSpeed", rb.linearVelocity.y);
        }
    }
    public interface IDownAttackBounceTarget
    {
        void OnDownAttackHit(Player player);
    }

    void ApplyBetterJump()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Z))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    void Jump()
    {
        IsGround = false;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (anim != null)
        {
            anim.SetBool("IsGround", false);
            anim.SetTrigger("JumpUp");
        }
    }
    void OnLanded()
    {
        if (isAttacking && isAirAttack)
        {
            EndAttackLock();
        }
    }
    public void PrepareForBossIntro()
    {
        canControl = false;

        inputX = 0f;
        jumpRequested = false;

        isDodging = false;
        dodgeTimer = 0f;

        EndAttackLock();
        ResetAttackTrigger();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("YSpeed", 0f);

            bool grounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );

            IsGround = grounded;
            anim.SetBool("IsGround", grounded);

            // Idle로 확실히 돌리고 싶으면 Idle state 이름에 맞게 사용
            // anim.Play("Idle", 0, 0f);
        }
    }

    private bool CanDodge()
    {
        return currentStamina >= dodgeCost;
    }

    private void UseStamina(int amount)
    {
        currentStamina = Mathf.Max(currentStamina - amount, 0);
        lastStaminaUseTime = Time.time;
        staminaRegenTimer = 0f;

        RefreshStaminaUI();
    }

    private void RegenerateStamina()
    {
        if (Time.time < lastStaminaUseTime + regenDelayAfterUse)
            return;

        if (currentStamina >= maxStamina)
            return;

        staminaRegenTimer += Time.deltaTime;

        if (staminaRegenTimer >= 1f / staminaRegenRate)
        {
            staminaRegenTimer = 0f;

            currentStamina = Mathf.Min(currentStamina + 1, maxStamina);

            RefreshStaminaUI();
        }
    }
    private void RefreshStaminaUI()
    {
        if (staminaUI != null)
            staminaUI.RefreshUI(currentStamina, maxStamina);
    }

    public void IncreaseMaxStamina(int amount)
    {
        maxStamina += amount;
        currentStamina += amount; // 최대 스태미너 증가 시 현재 스태미너도 같이 증가
        RefreshStaminaUI();
    }

    void TryDodge()
    {
        if (!IsGround) return;
        if (isDodging) return;
        if(Time.time < nextDodgeTime) return;

        if (!CanDodge()) return;

        // 공격 중 닷지 캔슬 허용하지 않는 경우
        if (isAttacking && !canDodgeCancelAttack) return;

        UseStamina(dodgeCost);

        StartDodge();
    }

    void StartDodge()
    {
        if (isAttacking)
        {
            EndAttackLock();
        }

        isDodging = true;
        dodgeTimer = dodgeDuration;
        nextDodgeTime = Time.time + dodgeCooldown;

        // hitbox off
        EndAttackLock();

        if (inputX > 0)
            dodgeDir = 1;
        else if (inputX < 0)
            dodgeDir = -1;
        else
            dodgeDir = isFacingRight ? 1 : -1;

        lockFacing = true;

        if(health != null)
            health.StartInvincible(dodgeInvincible);

        if (anim != null)
        {
            ResetAttackTrigger();
            anim.SetTrigger("Dodge");
            //if (VFXCheck)
            //    vfx.Play(VFX_Type.DodgeSlash);
        }
    }

    void ResetAttackTrigger()
    {
        if (anim == null) return;

        anim.ResetTrigger("Attack1");
        anim.ResetTrigger("Attack2");
        anim.ResetTrigger("Attack3");
        anim.ResetTrigger("AirAttack");
        anim.ResetTrigger("LowerAttack");
        anim.ResetTrigger("UpperAttack");
    }

    void DodgeMove()
    {
        dodgeTimer -= Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(dodgeDir * dodgeSpeed,0);

        if(dodgeTimer <= 0)
        {
            EndDodge();
        }
    }

    void EndDodge()
    {
        isDodging = false;
        lockFacing = false;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void GroundComboAttack()
    {
        StartAttackLock();
        isAirAttack = false;

        if(Time.time > comboExpireTime)
            comboIndex = 0; // 콤보 초기화

        comboIndex++;

        if(comboIndex > 3)
            comboIndex = 1; // 콤보 순환

        //lastAttackTime = Time.time;

        if (anim != null)
        {
            anim.SetTrigger("Attack" + comboIndex);
            if(VFXCheck)
                vfx.Play(VFX_Type.Slash);
        }
    }

    void AirSwingSide()
    {
        StartAttackLock();
        isAirAttack = true;

        if (anim != null)
        {
            anim.SetTrigger("AirAttack");
            if (VFXCheck)
                vfx.Play(VFX_Type.RangeSlash);
        }
    }

    void AirSwingDown()
    {
        StartAttackLock();
        isAirAttack = true;
        isLowerAttack = true;

        if (anim != null)
        {
            anim.SetTrigger("LowerAttack");
            if (VFXCheck)
                vfx.Play(VFX_Type.LowerSlash);
        }
    }

    void UpperAttack()
    {
        StartAttackLock();
        isAirAttack = !IsGround; // 땅이면 false, 공중이면 true

        if (anim != null)
        {
            anim.SetTrigger("UpperAttack");
            if (VFXCheck)
                vfx.Play(VFX_Type.UpperSlash);
        }
    }

    void StartAttackLock()
    {
        isAttacking = true;
        lockFacing = true;
    }

    void EndAttackLock()
    {
        isAttacking = false;
        lockFacing = false;
        isAirAttack = false;

        comboExpireTime = Time.time + comboResetTime;

        if (attack1Collider != null)
            attack1Collider.enabled = false;

        if (attack2Collider != null)
            attack2Collider.enabled = false;

        if (attack3Collider != null)
            attack3Collider.enabled = false;

        if (upperAttackCollider != null)
            upperAttackCollider.enabled = false;

        if (lowerAttackCollider != null)
            lowerAttackCollider.enabled = false;
    }

    public void AE_AttackEnd()
    {
        EndAttackLock();
    }

    public void AE_Attack1HitboxOn()
    {
        if (attack1Collider != null)
            attack1Collider.enabled = true;
    }

    public void AE_Attack2HitboxOn()
    {
        if (attack2Collider != null)
            attack2Collider.enabled = true;
    }

    public void AE_Attack3HitboxOn()
    {
        if (attack3Collider != null)
            attack3Collider.enabled = true;
    }

    public void AE_Attack1HitboxOff()
    {
        if (attack1Collider != null)
            attack1Collider.enabled = false;
    }

    public void AE_Attack2HitboxOff()
    {
        if (attack2Collider != null)
            attack2Collider.enabled = false;
    }

    public void AE_Attack3HitboxOff()
    {
        if (attack3Collider != null)
            attack3Collider.enabled = false;
    }

    public void AE_LowerHitboxOn()
    {
        if (lowerAttackCollider != null)
            lowerAttackCollider.enabled = true;
    }

    public void AE_LowerHitboxOff()
    {
        if (lowerAttackCollider != null)
            lowerAttackCollider.enabled = false;
    }
    public void AE_UpperHitboxOn()
    {
        if (upperAttackCollider != null)
            upperAttackCollider.enabled = true;
    }

    public void AE_UpperHitboxOff()
    {
        if (upperAttackCollider != null)
            upperAttackCollider.enabled = false;
    }

    public void AE_Rest()
    {
        if (currentRestPlace == null) return;

        currentRestPlace.ApplyRest(this);
        RestVFX.gameObject.SetActive(true);
    }

    public void AE_RestEnd()
    {
        RestVFX.gameObject.SetActive(false);
        canControl = true;
    }

    public void AE_FootStep()
    {
        if (!canControl) return;
        if (!IsGround) return;
        if (Mathf.Abs(inputX) < 0.1f) return;
        if (isDodging) return;
        if (isAttacking) return;
        if (health != null && health.IsKnockback) return;
        if (health != null && health.IsDead) return;

        playerAudio.PlayFootstep();
    }

    public void AE_AttackSound()
    {
        playerAudio.PlayAttack();
    }

    public void AE_DodgeSound()
    {
        playerAudio.PlayDodge();
    }

    public void AE_JumpSound()
    {
        playerAudio.PlayJump();
    }

    public void AE_LandSound()
    {
        playerAudio.PlayLand();
    }

    void Flip()
    {
        if (lockFacing) return;

        if (inputX > 0 && !isFacingRight)
        {
            isFacingRight = true;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (inputX < 0 && isFacingRight)
        {
            isFacingRight = false;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out RestPlace restplace))
        {
            currentRestPlace = restplace;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out RestPlace restplace))
        {
            if(currentRestPlace == restplace)
                currentRestPlace = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // GroundCheck Gizmo
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

}