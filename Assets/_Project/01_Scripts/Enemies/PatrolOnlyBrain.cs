using UnityEngine;

public class PatrolOnlyBrain : MonoBehaviour, IEnemyBrain
{
    public void Tick(EnemyCore core, float dt)
    {
        core.Movement?.SetEnabled(true);
    }
}
