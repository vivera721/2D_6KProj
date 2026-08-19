using UnityEngine;

public enum HeartHoarderState
{
    StaticIdle,
    WakeUp,
    Idle,
    Move,
    Acting,
    Vanish,
    Appear,
    Dead
}

public enum HeartHoarderAttackType
{
    None,
    GroundDashSlash,
    AirDashSlash,
    StationarySpinSlash,
    AirSlam
}