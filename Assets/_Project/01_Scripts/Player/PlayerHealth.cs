using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 5;
    public int currentHP;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0;

    [Header("Invincible")]
    public float invincibleTime = 0.2f;
    bool invincible;

    [Header("BodyKnockBack")]
    public float bodyknockbackForce = 5f;
    public float bodyUpForce = 1.5f;

    [Header("Knockback Lock")]
    [SerializeField] float knockLockTime = 0.15f;
    public bool IsKnockback { get; private set; }   //  Player가 읽을 값

    Rigidbody2D rb;
    CapsuleCollider2D capsuleCollider;
    Player player;
    Coroutine knockCo;

    PlayerHitFlash hitFlash;
    public PlayerHPUI playerHPUI;
    Animator anim;

    public event Action<int, int> OnHPChanged;
    public event Action OnDead;

    [Header("UI Shakes")]
    [SerializeField] private DOTweenAnimation HP_UI_Animation;
    [SerializeField] private DOTweenAnimation ST_UI_Animation;
    [SerializeField] private CameraShake cameraShake;

    [Header("Sound Effects")]
    [SerializeField] private PlayerAudio playerAudio;

    [Header("GameOver")]
    [SerializeField] private CanvasGroup gameOverPanelGroup;
    [SerializeField] private CanvasGroup gameOverTextGroup;
    [SerializeField] private float gameOverSlowDuration = 0.5f;
    [SerializeField] private float gameOverPanelDelay = 1f;
    [SerializeField] private float gameOverFadeDuration = 1f;
    [SerializeField] private float gameOverTextDelay = 1f;
    [SerializeField] private float gameOverTextFadeDuration = 1f;
    [SerializeField] private float loadMainMenuDelay = 1f;

    void Awake()
    {
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        hitFlash = GetComponent<PlayerHitFlash>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        playerAudio = GetComponent<PlayerAudio>();
    }

    private void Start()
    {
        TryApplyRuntimeData();
        TryLoadHPStatus();
        NotifyHPChanged();
    }
    private void TryApplyRuntimeData()
    {
        if (PlayerRuntimeData.Instance == null) return;
        if (!PlayerRuntimeData.Instance.HasData) return;

        ApplyRuntimeHP(
            PlayerRuntimeData.Instance.maxHP,
            PlayerRuntimeData.Instance.currentHP
        );
    }
    public void ApplyRuntimeHP(int savedMaxHP, int savedCurrentHP)
    {
        if (savedMaxHP <= 0) return;

        maxHP = savedMaxHP;
        currentHP = Mathf.Clamp(savedCurrentHP, 0, maxHP);

        NotifyHPChanged();
    }
    private void TryLoadHPStatus()
    {
        if (SaveManager.Instance == null) return;
        if (!SaveManager.Instance.IsContinueMode) return;
        if (!SaveManager.Instance.HasSaveData()) return;

        int savedMaxHP = SaveManager.Instance.LoadMaxHP();
        int savedCurrentHP = SaveManager.Instance.LoadCurrentHP();

        if (savedMaxHP <= 0) return;

        maxHP = savedMaxHP;
        currentHP = Mathf.Clamp(savedCurrentHP, 0, maxHP);

        NotifyHPChanged();
    }

    public void TakeDamage(int damage)
    {
        if(IsDead) return;
        if(damage <= 0) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        NotifyHPChanged();

        if (currentHP > 0) 
        {
            playerAudio.PlayHit();
            hitFlash.PlayFlash();
            DoShake();
            StartCoroutine(HitStop());
            // anim.SetTrigger("Damaged");
        }
        else if (currentHP <= 0f)
        {
            Die();
            StartCoroutine(GameOver());
        }
    }

    IEnumerator GameOver()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // 0.5초 동안 timeScale 1 -> 0
        float timer = 0f;

        while (timer < gameOverSlowDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / gameOverSlowDuration;
            Time.timeScale = Mathf.Lerp(1f, 0f, t);

            // 물리도 같이 부드럽게 느려지게 함
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return null;
        }

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f;


        if (gameOverPanelGroup == null || gameOverTextGroup == null)
        {
            yield return new WaitForSecondsRealtime(2f);
            SceneManager.LoadScene("Main Menu");
            yield break;
        }

        gameOverPanelGroup.alpha = 0f;
        gameOverTextGroup.alpha = 0f;

        gameOverPanelGroup.blocksRaycasts = true;
        gameOverPanelGroup.interactable = true;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); // Time.timeScale 영향 안 받게 함

        seq.AppendInterval(gameOverPanelDelay);
        seq.Append(gameOverPanelGroup.DOFade(1f, gameOverFadeDuration));
        seq.AppendInterval(gameOverTextDelay);
        seq.Append(gameOverTextGroup.DOFade(1f, gameOverTextFadeDuration));
        seq.AppendInterval(loadMainMenuDelay);

        yield return seq.WaitForCompletion();

        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    private void DoShake()
    {
        HP_UI_Animation.DORestart();
        ST_UI_Animation.DORestart();
        cameraShake.ShakeNormal();
    }

    IEnumerator HitStop()
    {
        if (IsDead) yield break;

        Time.timeScale = 0.25f;
        yield return new WaitForSecondsRealtime(0.05f);

        if (!IsDead)
            Time.timeScale = 1f;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (IsDead) return;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        NotifyHPChanged();
    }

    public void SetMaxHP(int newMaxHP, bool fillCurrentHP = true)
    {
        if (newMaxHP <= 0) return;

        maxHP = newMaxHP;

        if (fillCurrentHP)
            currentHP = maxHP;
        else
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        NotifyHPChanged();
    }

    public void IncreaseMaxHP(int amount, bool healAddedAmount = true)
    {
        if(amount <= 0) return;

        int oldMaxHP = maxHP;
        maxHP += amount;

        if (healAddedAmount) 
            currentHP += (maxHP - oldMaxHP);

        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        NotifyHPChanged();
    }

    public void Die()
    {
        OnDead?.Invoke();

        DisableComponents();

        anim.SetTrigger("Die");
        Debug.Log("Player Died", this);
    }

    public void DisableComponents()
    {
        // 플레이어 조작, 공격, 충돌 등 비활성화
        if (player != null)
        {
            player.moveSpeed = 0f;
            player.enabled = false;
        }

        capsuleCollider.excludeLayers = LayerMask.GetMask("Enemy", "EnemyAttack");
        hitFlash.enabled = false;
        //if (capsuleCollider != null) capsuleCollider.enabled = false;
    }

    private void NotifyHPChanged()
    {
        playerHPUI.RefreshUI(currentHP, maxHP);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void StartInvincible(float time)
    {
        if(time <= 0) return;


        StartCoroutine(InvincibleRoutine(time));
    }

    IEnumerator InvincibleRoutine(float time)
    {
        invincible = true;
        if(capsuleCollider != null)
            capsuleCollider.excludeLayers = LayerMask.GetMask("Enemy", "EnemyAttack");

        yield return new WaitForSeconds(time);

        if (capsuleCollider != null)
            capsuleCollider.excludeLayers = LayerMask.GetMask("Nothing");
        invincible = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (invincible) return;

        if (collision.TryGetComponent(out DamageDealer dealer))
        {
            if (dealer.owner == transform.root) return;
            if (dealer.ownerType != DamageOwner.Enemy) return;

            ApplyHit(dealer, collision.transform);
            return;
        }

        // 예: 적 본체 Hurtbox에 플레이어 AttackCollider가 닿는 상황
        if (collision.CompareTag("Enemy"))
            return;


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (invincible) return;

        // 적 몸박 충돌
        if (collision.collider.CompareTag("Enemy"))
        {
            TakeDamage(1); // 몸박데미지
            KnockbackFrom(collision.transform.position, bodyknockbackForce, bodyUpForce);
            StartCoroutine(InvincibleRoutine(invincibleTime));

            if (player != null)
            {
                hitFlash.PlayFlash();
            }
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (invincible) return;

    //    // 2) 적 몸통 충돌: Enemy 태그로 넉백만(또는 데미지도)
    //    if (collision.collider.CompareTag("Enemy"))
    //    {
    //        KnockbackFrom(collision.transform.position, bodyknockbackForce, bodyUpForce);
    //        StartCoroutine(InvincibleRoutine());
    //        if (player != null)
    //        {
    //            hitFlash.PlayFlash();
    //            //player.anim.SetTrigger("Damaged");
    //        }
    //    }
    //}

    public bool TakeEnemyHit(int damage, Vector3 hitPosition, float knockbackForce, float knockbackUpForce)
    {
        if(IsDead) return false;
        if (invincible) return false;
        if(damage <= 0) return false;

        TakeDamage(damage);
        if (!IsDead)
        {
            KnockbackFrom(hitPosition, knockbackForce, knockbackUpForce);
            StartCoroutine(InvincibleRoutine(invincibleTime));
        }

        return true;
    }

    void ApplyHit(DamageDealer dealer, Transform attacker)
    {
        Vector3 hitOrigin = dealer.owner != null ? dealer.owner.position : attacker.position;

        TakeEnemyHit(dealer.damage, hitOrigin, dealer.knockbackForce, dealer.knockbackUpForce);
    }

    void KnockbackFrom(Vector3 attackerPos, float force, float upForce)
    {
        float dx = transform.position.x - attackerPos.x;
        int dirSign = Mathf.Abs(dx) < 0.0001f
            ? (transform.localScale.x >= 0f ? 1 : -1)
            : (dx > 0f ? 1 : -1);

        // 기존 속도 중 X만 초기화
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // ✅ 이동 덮어쓰기 방지(넉백 동안 입력 이동 잠금)
        StartKnockbackLock(knockLockTime);

        // 임펄스 적용
        rb.AddForce(new Vector2(dirSign * force, upForce), ForceMode2D.Impulse);
    }

    void StartKnockbackLock(float t)
    {
        if (knockCo != null) StopCoroutine(knockCo);
        knockCo = StartCoroutine(KnockbackRoutine(t));
    }

    IEnumerator KnockbackRoutine(float t)
    {
        IsKnockback = true;
        yield return new WaitForSeconds(t);
        IsKnockback = false;
    }

}
