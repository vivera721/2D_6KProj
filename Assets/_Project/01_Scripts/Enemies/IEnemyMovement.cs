using UnityEngine;

public interface IEnemyMovement
{
    // every frame movement logic ( chase , patrol , hover , etc )
    void Tick(EnemyCore core, float dt);

    // ( Option ) Move Lock while Attack , Stop when damaged
    void SetEnabled(bool enabled);
}
