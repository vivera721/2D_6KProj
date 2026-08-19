using UnityEngine;

[CreateAssetMenu(menuName = "2D6K/Enemy/Attack Profile", fileName = "AP_Enemy_")]
public class EnemyAttackProfile : ScriptableObject
{
    [Header("Common")]
    public float attackCooltime = 2f;
    public float windUpTime = 0f;

    [Header("Animator Triggers")]
    public string meleeTrigger = "Melee";
    public string sweepTrigger = "Sweep";
    public string afterSweepTrigger = "AfterSweep";
    public string beamTrigger = "Beam";
    public string healTrigger = "Heal";

    [Header("Enable Flags")]
    public bool enableMelee = true;
    public bool enableSweep = true;
    public bool enableBeam= true;
    public bool enableHeal = true;
    public bool enableSpin = true;

    [Header("Melee")]
    public float meleeRange = 1.7f;

    [Header("Sweep")]
    public float sweepRange = 4.5f;
    [Space(5)]
    public float dashSpeed = 20f;
    public float dashDuration = 0.08f;
    public float dashMaxDistance = 3.5f;

    [Header("Beam / Shockwave")]
    public float beamRange = 5f;
    [Space(5)]
    public float beamActiveTime = 0.12f;
    public bool beamBlockedByWall = false;

    [Header("Heal")]
    public float healCastTime = 0.5f;
    public int healAmount = 20;
    [Range(0f, 1f)] public float healRatio = 0.2f;     // MaxHP 비율 회복용
    public bool useHealRatio = false;

    [Tooltip("현재 체력이 이 비율 이하일 때만 힐 사용")]
    [Range(0f, 1f)] public float healBelowHpRatio = 0.35f;

    [Tooltip("타겟과 이 거리 이상 벌어졌을 때만 힐")]
    public float healMinDistanceFromTarget = 3f;

    [Tooltip("힐 전용 쿨타임")]
    public float healCooldown = 8f;

    [Tooltip("회복 중 피격되면 취소할지")]
    public bool cancelHealOnHit = true;

    [Header("Combo Chances (0~1)")]
    [Range(0f, 1f)] public float slamToSpinChance = 0.0f;
    [Range(0f, 1f)] public float sweepToSpinChance = 1.0f;


    [Header("Combo Chances (0~1)")]
    [Range(0f, 1f)] public float meleeToSweepChance = 0.0f;
    [Range(0f, 1f)] public float sweepToBeamChance = 0.0f;

    /*
     * 근거리 Only는 enableSweep=false만 하면 끝
     * 돌진 Only는 enableSlam=false, enableSpin=false + choose 규칙만 약간 바꾸면 됨(혹은 그냥 sweep만 선택되게)
     */
}