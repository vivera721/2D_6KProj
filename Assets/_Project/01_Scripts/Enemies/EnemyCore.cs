using UnityEngine;

[DisallowMultipleComponent]
public class EnemyCore : MonoBehaviour
{
    // 필수 참조 모음
    // 상태 흐름 관리
    // 모듈 호출 순서 제어

    [Header("References")]
    public Animator animator { get; private set; }
    public Transform target { get; private set; }
    public IEnemyBrain Brain { get; private set; }
    public IEnemyMovement Movement { get; private set; }
    public IEnemyAttack Attack { get; private set; }
    public EnemyHealth Health { get; private set; }
    public int Facing { get; private set; } = 1; // 1 = right, -1 = left

    private void Awake()
    {
        animator = GetComponent<Animator>();
        target = FindFirstObjectByType<Player>().transform;

        Brain = GetComponent<IEnemyBrain>();
        Movement = GetComponent<IEnemyMovement>();
        Attack = GetComponent<IEnemyAttack>();
        //Debug.Log($"Attack module found? {Attack != null}", this);
    }

    private void Update()
    {
        //Debug.Log("EnemyCore Update running", this);

        float dt = Time.deltaTime;

        // Brain이 없으면 “그냥 이동/공격 모듈이 자체 판단”하게 두는 것도 가능
        Brain.Tick(this, dt);

        // 또는 Brain에서 명령 내려도, Movement/Attack은 자체적으로도 Tick 가능

        Movement?.Tick(this, dt);
        Attack?.Tick(this, dt);
    }

    public void SetFacing(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;

        int newFacing = dirX > 0 ? 1 : -1;
        if (Facing == newFacing) return;

        Facing = newFacing;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Facing;
        transform.localScale = scale;
    }
}