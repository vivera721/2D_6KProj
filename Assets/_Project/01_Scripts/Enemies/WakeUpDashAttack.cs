using UnityEngine;

public class WakeUpDashAttack : MonoBehaviour, IEnemyAttack
{
    public enum AttackType
    {
        None,
        Melee,
        Sweep,
        AfterSweep,
        Beam,
        Heal
    }

    private enum Phase
    {
        None,
        WindUp,
        Active,
        Dash
    }

    [Header("Profile")]
    public EnemyAttackProfile profile;

    [Header("Hitbox Colliders")]
    public BoxCollider2D baseCollider;
    public BoxCollider2D sweepCollider; 
    public BoxCollider2D beamCollider;

    private EnemyCore core;

    private float lastAttackTime = -999f;
    private float lastHealTime = -999f;

    private AttackType currentAttack = AttackType.None;
    private AttackType? queuedAttack = null;

    private Phase phase = Phase.None;
    private float windUpTimer = 0f;

    private float dashTimer = 0f;
    private Vector3 dashStartPos;

    public bool IsAttacking { get; private set; }

    private int meleeHash;
    private int sweepHash;
    private int afterSweepHash;
    private int beamHash;
    private int healHash;

    private void Awake()
    {
        core = GetComponent<EnemyCore>();
        DisableAllHitboxes();
        RebuildAnimatorHashes();
    }

    private void OnValidate()
    {
        RebuildAnimatorHashes();
    }

    private void RebuildAnimatorHashes()
    {
        if (profile == null) return;

        meleeHash = Animator.StringToHash(profile.meleeTrigger);
        sweepHash = Animator.StringToHash(profile.sweepTrigger);
        afterSweepHash = Animator.StringToHash(profile.afterSweepTrigger);
        beamHash = Animator.StringToHash(profile.beamTrigger);
        healHash = Animator.StringToHash(profile.healTrigger);
    }

    private void DisableAllHitboxes()
    {
        if (baseCollider != null)
            baseCollider.enabled = false;
        if (sweepCollider != null)
            sweepCollider.enabled = false;
        if (beamCollider != null)
            beamCollider.enabled = false;
    }

    public bool CanAttack(EnemyCore c)
    {
        if (profile == null) return false;
        if (c == null || c.target == null) return false;
        if (IsAttacking) return false;

        return Time.time >= lastAttackTime + profile.attackCooltime;
    }

    public void Execute(EnemyCore c)
    {
        if (!CanAttack(c)) return;

        core = c;
        DisableAllHitboxes();

        queuedAttack = null;
        phase = Phase.WindUp;
        windUpTimer = 0f;
        dashTimer = 0f;

        float dx = core.target.position.x - core.transform.position.x;
        core.SetFacing(dx);

        currentAttack = ChooseAttack(core);

        if (currentAttack == AttackType.None)
            return;

        IsAttacking = true;
        lastAttackTime = Time.time;

        if (currentAttack == AttackType.Heal)
            lastHealTime = Time.time;

        // 콤보 예약
        if (currentAttack == AttackType.Sweep &&
            profile.enableSpin &&
            Random.value < profile.sweepToSpinChance)
        {
            queuedAttack = AttackType.AfterSweep;
        }

        if (currentAttack == AttackType.Melee &&
            profile.enableSpin &&
            Random.value < profile.slamToSpinChance)
        {
            queuedAttack = AttackType.AfterSweep;
        }

        if (profile.windUpTime <= 0f)
        {
            phase = Phase.Active;
            PlayAttackAnim(currentAttack);
        }
        else
        {
            core.Movement?.SetEnabled(false);
        }
    }

    public void Tick(EnemyCore c, float dt)
    {
        if (!IsAttacking) return;

        if (core == null)
            core = c;

        if (core == null || profile == null) return;

        core.Movement?.SetEnabled(false);

        if (phase == Phase.WindUp)
        {
            windUpTimer += dt;
            if (windUpTimer >= profile.windUpTime)
            {
                phase = Phase.Active;
                PlayAttackAnim(currentAttack);
            }
            return;
        }

        if (phase == Phase.Dash)
        {
            dashTimer += dt;

            Vector3 dir = new Vector3(core.Facing, 0f, 0f);

            float moved = Vector3.Distance(dashStartPos, core.transform.position);
            if (moved < profile.dashMaxDistance)
            {
                core.transform.position += dir * profile.dashSpeed * dt;
            }

            if (sweepCollider != null && sweepCollider.enabled)
            {
                UpdateSweepTrailHitbox(core);
            }

            if (dashTimer >= profile.dashDuration)
            {
                EndDash();
            }
        }
    }

    private AttackType ChooseAttack(EnemyCore c)
    {
        if (c == null || c.target == null || profile == null)
            return AttackType.None;

        float dist = Vector2.Distance(c.transform.position, c.target.position);

        // 1. Heal 우선 체크
        if (CanUseHeal(c))
            return AttackType.Heal;

        // 2. 근거리
        if (dist <= profile.meleeRange)
        {
            if (profile.enableMelee)
                return AttackType.Melee;

            if (profile.enableSpin)
                return AttackType.AfterSweep;
        }

        // 3. Beam
        if (profile.enableBeam && dist <= profile.beamRange)
        {
            return AttackType.Beam;
        }

        // 4. Sweep
        if (dist <= profile.sweepRange && profile.enableSweep)
        {
            return AttackType.Sweep;
        }

        // fallback
        if (profile.enableSweep) return AttackType.Sweep;
        if (profile.enableBeam) return AttackType.Beam;
        if (profile.enableMelee) return AttackType.Melee;
        if (profile.enableSpin) return AttackType.AfterSweep;

        return AttackType.None;
    }

    private bool CanUseHeal(EnemyCore c)
    {
        if (profile == null || !profile.enableHeal) return false;
        if (c == null || c.Health == null) return false;

        float hpRatio = (float)c.Health.CurrentHP / c.Health.MaxHp;
        if (hpRatio > profile.healBelowHpRatio) return false;

        if (c.target != null)
        {
            float dist = Vector2.Distance(c.transform.position, c.target.position);
            if (dist < profile.healMinDistanceFromTarget) return false;
        }

        if (Time.time < lastHealTime + profile.healCooldown) return false;

        return true;
    }
    public bool CanUseHealPublic(EnemyCore c)
    {
        return CanUseHeal(c);
    }
    public bool TryStartAttackExternal(AttackType type, EnemyCore c)
    {
        if (profile == null || IsAttacking) return false;
        if (Time.time < lastAttackTime + profile.attackCooltime) return false;
        if (c == null) return false;

        core = c;
        currentAttack = type;
        queuedAttack = null;
        phase = Phase.WindUp;
        windUpTimer = 0f;
        dashTimer = 0f;

        DisableAllHitboxes();

        if (core.target != null)
            core.SetFacing(core.target.position.x - core.transform.position.x);

        IsAttacking = true;
        lastAttackTime = Time.time;

        if (currentAttack == AttackType.Heal)
            lastHealTime = Time.time;

        if (profile.windUpTime <= 0f)
        {
            phase = Phase.Active;
            PlayAttackAnim(currentAttack);
        }
        else
        {
            core.Movement?.SetEnabled(false);
        }

        return true;
    }

    public void OnDamaged()
    {
        if (!IsAttacking || profile == null) return;

        if (currentAttack == AttackType.Heal && profile.cancelHealOnHit)
        {
            CancelCurrentAttack();
        }
    }

    private void CancelCurrentAttack()
    {
        DisableAllHitboxes();

        queuedAttack = null;
        currentAttack = AttackType.None;
        phase = Phase.None;
        IsAttacking = false;
        dashTimer = 0f;
        windUpTimer = 0f;
    }

    private void PlayAttackAnim(AttackType type)
    {
        if (core == null || core.animator == null || profile == null) return;

        switch (type)
        {
            case AttackType.Melee:
                core.animator.SetTrigger(meleeHash);
                break;

            case AttackType.Sweep:
                core.animator.SetTrigger(sweepHash);
                break;

            case AttackType.AfterSweep:
                core.animator.SetTrigger(afterSweepHash);
                break;

            case AttackType.Beam:
                core.animator.SetTrigger(beamHash);
                break;

            case AttackType.Heal:
                core.animator.SetTrigger(healHash);
                break;
        }
    }

    // =========================
    // Animation Events
    // =========================

    public void AE_ApplyHeal()
    {
        if (!IsAttacking || profile == null) return;
        if (currentAttack != AttackType.Heal) return;
        if (core == null || core.Health == null) return;

        int amount = profile.useHealRatio
            ? Mathf.RoundToInt(core.Health.MaxHp * profile.healRatio)
            : profile.healAmount;

        core.Health.Heal(amount);
    }

    public void AE_HitboxOn()
    {
        if (!IsAttacking || profile == null || core == null) return;

        //if (core.target != null)
        //{
        //    core.SetFacing(core.target.position.x - core.transform.position.x);
        //}

        DisableAllHitboxes();

        switch (currentAttack)
        {
            case AttackType.Melee:
            case AttackType.AfterSweep:
                {
                    if (baseCollider == null) return;
                    baseCollider.enabled = true;
                    break;
                }

            case AttackType.Sweep:
                {
                    if (sweepCollider == null) return;
                    sweepCollider.enabled = true;
                    UpdateSweepTrailHitbox(core);
                    break;
                }

            case AttackType.Beam:
                {
                    if (beamCollider == null) return;

                    beamCollider.enabled = true;

                    // 벽에 막히는 Beam을 원하면 여기서 Raycast 기반 길이 보정 추가
                    break;
                }
        }
    }

    public void AE_HitboxOff()
    {
        DisableAllHitboxes();
    }

    public void AE_SweepDashStart()
    {
        if (!IsAttacking || profile == null) return;
        if (currentAttack != AttackType.Sweep) return;

        phase = Phase.Dash;
        dashTimer = 0f;
        dashStartPos = core != null ? core.transform.position : transform.position;

        if (sweepCollider != null && sweepCollider.enabled)
        {
            UpdateSweepTrailHitbox(core);
        }
    }

    public void AE_SweepDashEnd()
    {
        if (!IsAttacking) return;
        if (currentAttack != AttackType.Sweep) return;

        EndDash();
    }

    public void AE_AttackEnd()
    {
        DisableAllHitboxes();
        phase = Phase.None;

        if (queuedAttack.HasValue)
        {
            currentAttack = queuedAttack.Value;
            queuedAttack = null;

            phase = Phase.Active;
            IsAttacking = true;

            PlayAttackAnim(currentAttack);
            return;
        }

        currentAttack = AttackType.None;
        IsAttacking = false;
    }

    private void EndDash()
    {
        if (phase != Phase.Dash) return;

        phase = Phase.Active;
        dashTimer = 0f;
    }

    // =========================
    // Sweep Trail Hitbox
    // =========================

    private void UpdateSweepTrailHitbox(EnemyCore c)
    {
        if (c == null || sweepCollider == null || profile == null) return;

        Vector2 a = dashStartPos;
        Vector2 b = c.transform.position;

        float length = Vector2.Distance(a, b);
        Vector2 center = (a + b) * 0.5f;

        Vector2 localCenter = c.transform.InverseTransformPoint(center);

        sweepCollider.offset = localCenter;
    }
}