using UnityEngine;

public interface IEnemyAttack
{
    // every frame attack decision / cooltime process
    void Tick(EnemyCore core, float dt);

    // Can Attack Now ( good to use when brain decide "Attack!" )
    bool CanAttack(EnemyCore core);

    // Brain commands execute of Attack
    void Execute(EnemyCore core);

    bool IsAttacking { get; }  
}
