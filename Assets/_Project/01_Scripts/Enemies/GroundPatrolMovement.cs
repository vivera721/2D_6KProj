using UnityEngine;

public class GroundPatrolMovement : MonoBehaviour, IEnemyMovement
{
    [Header("Patrol Point")]
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Rest")]
    public float moveDuration = 3.5f;
    public float restDuration = 0.5f;

    bool movingRight = true;
    bool isResting = false;

    float moveTimer = 0f;
    float restTimer = 0f;
    float nextRestTime;

    bool enabledMovement = true;

    //public void SetEnabled(bool enabled) => enabledMovement = enabled;

    private void Start()
    {
        ScheduleNextRest();
    }

    void ScheduleNextRest()
    {
        //nextRestTime = Time.time + Random.Range(3f, 4f);
        nextRestTime = Time.time + moveDuration;
    }

    public void Tick(EnemyCore core, float dt)
    {
        // 이동 처리
        if (!enabledMovement)
        {
            core.animator?.SetBool("IsMoving", false); 
            return; 
        }


        if (isResting)
        {
            HandleRest(core,dt);
        }
        else
        {
            HandleMove(core, dt);
        }
    }

    void HandleMove(EnemyCore core, float dt)
    {
        if(Time.time >= nextRestTime)
        {
            isResting = true;
            restTimer = 0f;
            core.animator.SetBool("IsMoving", false);
            return;
        }

        moveTimer += dt;

        Vector3 target = movingRight ? rightPoint.position : leftPoint.position;

        float dirX = target.x - transform.position.x;

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * dt);

        // 방향
        core.SetFacing(dirX);

        core.animator.SetBool("IsMoving", true);

        float arriveDist = 0.05f;
        if (Vector3.Distance(transform.position, target) <= arriveDist)
        {
            movingRight = !movingRight;
            return;
        }

        //if(moveTimer >= moveDuration)
        //{
        //    moveTimer = 0f;
        //    isResting = true;
        //    restTimer = 0f;

        //    core.animator.SetBool("IsMoving", false);
        //}

    }


    void HandleRest(EnemyCore core, float dt)
    {
        restTimer += dt;
        core.animator.SetBool("IsMoving", false);

        if (restTimer >= restDuration)
        {
            isResting = false;
            ScheduleNextRest();
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (enabledMovement == enabled) return;
        enabledMovement = enabled;
        Debug.Log($"[GroundPatrolMovement] SetEnabled = {enabled}", this);

        if (!enabled)
        {
            isResting = false;
            moveTimer = 0f;
            restTimer = 0f;
        }
        else
        {
            isResting = false;
            restTimer = 0f;
            moveTimer = 0f;
            ScheduleNextRest();
        }
    }

}
