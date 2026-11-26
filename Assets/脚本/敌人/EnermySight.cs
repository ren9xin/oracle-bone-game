using UnityEngine;

public class EnemySight : MonoBehaviour
{
    [Header("扇形视野")]
    public float sightAngle = 60f;
    public float sightRadius = 4f;
    public LayerMask obstacleLayer;

    private Transform player;
    private 隐身 p;
    private EnemyPatrol patrol;
    public bool canSeePlayer;

    private bool isUpperFloor;
    private bool hasSpottedPlayer;

    //开局延迟，防止刚运行就误触发
    private float startDelay = 0.2f;
    private float startTime;

    void Start()
    {
        startTime = Time.time; // 记录开局时间
        player = GameObject.FindGameObjectWithTag("Player").transform;
        p = player.GetComponent<隐身>();
        patrol = GetComponent<EnemyPatrol>();
        // ========================
        // 识别自己是上层还是下层士兵
        // ========================
        isUpperFloor = gameObject.layer == LayerMask.NameToLayer("EnemyUpper");

    }

    void Update()
    {
        // 前 0.2 秒不检测
        if (Time.time < startTime + startDelay)
            return;
        canSeePlayer = CheckSight();
        if (canSeePlayer)
        {
            patrol.StartChase();

            // ========================
            // 只在第一次发现时通知同层士兵
            // ========================
            if (!hasSpottedPlayer)
            {
                hasSpottedPlayer = true;
                EnemyEvents.PlayerSpotted(player, isUpperFloor);
            }
        }
        else
        {
            // 失去目标，重置
            hasSpottedPlayer = false;
        }
    }

    bool CheckSight()
    {
        Vector2 dir = player.position - transform.position;
        float dist = dir.magnitude;

        if (dist > sightRadius || p.isFaded) return false;

        float dot = Vector2.Dot(Vector2.right * patrol.faceDir, dir.normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        if (angle > sightAngle / 2) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dist, obstacleLayer);
        return !hit;
    }

    void OnDrawGizmosSelected()
    {
        // 空判断，编辑器下不报错
        if (player == null) return;

        Gizmos.color = canSeePlayer ? Color.red : Color.green;
        Vector2 fwd = Vector2.right * patrol.faceDir * sightRadius;
        Vector2 r = Quaternion.Euler(0, 0, sightAngle / 2) * fwd;
        Vector2 l = Quaternion.Euler(0, 0, -sightAngle / 2) * fwd;

        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + r);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + l);
    }
}
