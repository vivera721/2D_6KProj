using UnityEngine;

public enum BloodKingState
{
    StaticIdle,
    WakeUp,
    WakeUpVanish,
    WakeUpAppear,
    Idle,
    Move,
    Acting,
    Vanish,
    Appear,
    Finisher,
    Dead
}

public enum BloodKingAttackType
{
    None,
    DodgeChargeSlash,
    DoubleSlash,
    JumpSlam,
    StabAndSpin
}