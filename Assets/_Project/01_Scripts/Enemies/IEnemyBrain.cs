using UnityEngine;

public interface IEnemyBrain
{
    // Decide " What To Do " ( Move , Attack , Hold , Fall Back ) 
    // 포인트: Brain은 “결정만” 하고, 실제 행동(이동/공격)은 Movement/Attack이 수행.
    void Tick(EnemyCore core, float dt);
}
