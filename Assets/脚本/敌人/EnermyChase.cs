using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("追击")]
    public float chaseSpeed = 3.5f;
    public float alertRange = 20f;//呼叫alertRange范围内的所有士兵
    public float stopRange = 4f;//大于该范围停止追击

    [Header("攻击")]
    public float attackRange = 1.2f;
    public float attackDuration = 0.5f;  // 攻击动作时长
    public float attackCooldown = 1.5f;  // 攻击冷却

    [Header("动画")]
    public Animator anim;

    private Transform player;
    private EnemyPatrol patrol;

    private float attackTimer;
    private bool isAttacking;
    private float attackCounter;

    // ========================
    // 记录自己属于哪一层
    // ========================
    private bool isUpperFloor;

    // 警报后的缓冲时间
    private float alertedIgnoreStopTime;
    public float alertIgnoreDuration = 3f; // 前3秒不脱战

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        patrol = GetComponent<EnemyPatrol>();
        if (anim == null) anim = GetComponent<Animator>();

        EnemyEvents.OnPlayerSpotted += OnSpotted;
        attackTimer = attackCooldown;
        // ========================
        // 识别自己是上层还是下层士兵
        // ========================
        isUpperFloor = gameObject.layer == LayerMask.NameToLayer("EnemyUpper");
    }

    void OnDestroy()
    {
        EnemyEvents.OnPlayerSpotted -= OnSpotted;
    }

    void OnSpotted(Transform target, bool senderisUpper)
    {
        if (senderisUpper != isUpperFloor) return;
        float dist = Vector2.Distance(transform.position, target.position);
        Debug.Log(dist);
        Debug.Log(alertRange);
        if (dist <= alertRange)
        {
            patrol.StartChase();
            alertedIgnoreStopTime = Time.time + alertIgnoreDuration;
        }
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (player == null || patrol == null) return;
        if (patrol.isChasing) ChasePlayer();
    }

    void ChasePlayer()
    {
        // 攻击中：完全不动
        if (isAttacking)
        {
            attackCounter += Time.deltaTime;
            if (attackCounter >= attackDuration)
            {
                EndAttack();
            }
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        float dirX = player.position.x - transform.position.x;

        // ========================
        // 核心防抖：转向盲区 + 延迟转向
        // ========================
        float flipDeadZone = 0.2f; // 盲区宽度，越大越稳
        if (Mathf.Abs(dirX) > flipDeadZone)
        {
            if (dirX > 0 && patrol.faceDir < 0)
                patrol.Flip();
            else if (dirX < 0 && patrol.faceDir > 0)
                patrol.Flip();
        }

        //脱战
        if (Time.time < alertedIgnoreStopTime)
        {
            // 缓冲期内：不判断脱战
        }
        else
        {
            // 缓冲结束：正常脱战
            if (distToPlayer > stopRange)
            {
                patrol.StopChase();
                return;
            }
        }
        // 可以攻击
        if (distToPlayer <= attackRange && attackTimer >= attackCooldown)
        {
            StartAttack();
            return;
        }

        // ==============================================
        // 核心优化：攻击CD期间，在攻击范围内就站着不动
        // ==============================================
        if (distToPlayer <= attackRange - 0.2f)
        {
            // 站在原地看着玩家，不抖动、不左右横跳
            anim.SetBool("IsRun", false);
            return;
        }

        // 跑步追击
        anim.SetBool("IsRun", true);
        anim.SetBool("IsAttack", false);
        transform.Translate(Vector2.right * patrol.faceDir * chaseSpeed * Time.deltaTime);
    }

    void StartAttack()
    {
        isAttacking = true;
        attackCounter = 0f;

        // 播放攻击动画
        anim.SetBool("IsRun", false);
        anim.SetBool("IsAttack", true);
    }

    void EndAttack()
    {
        isAttacking = false;
        attackTimer = 0f; // 进入CD

        // 停止攻击动画
        anim.SetBool("IsAttack", false);
    }
}