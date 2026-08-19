using UnityEngine;

public enum GolemState
{
    StaticIdle,
    WakeUp,
    Idle,
    Move,
    Attack,
    Dead
}

public enum GolemAttackType
{
    None,
    Spin,
    HandSlam,
    Beam,
    Burst,
    Buff
}