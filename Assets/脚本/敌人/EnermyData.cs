using UnityEngine;
using System.Collections;
public class EnemyData : MonoBehaviour
{
    [Header("血量设置")]
    public int maxHealth = 50; // 最大血量
    private int currentHealth; // 当前血量

    [Header("受击设置")]
    public float hitInvulnerableTime = 0.5f; // 受击无敌时间
    private bool isInvulnerable; // 是否处于无敌状态
    private float invulnerableTimer; // 无敌计时器

    [Header("动画组件")]
    public Animator anim; // 敌人动画组件

    // 血量改变事件（可选，用于血条UI等扩展）
    public System.Action<int, int> OnHealthChanged;
    // 死亡事件
    public System.Action OnEnemyDead;

    private void Awake()
    {
        // 初始化动画组件（如果未手动赋值）
        if (anim == null)
            anim = GetComponent<Animator>();

        // 初始化血量
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // 无敌时间倒计时
        if (isInvulnerable)
        {
            invulnerableTimer -= Time.deltaTime;
            if (invulnerableTimer <= 0)
                isInvulnerable = false;
        }
    }

    /// <summary>
    /// 受击函数（外部调用，比如玩家攻击时调用）
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    public void TakeDamage(int damage)
    {
        // 无敌/死亡状态下不受击
        if (isInvulnerable || currentHealth <= 0)
            return;

        // 扣血
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // 触发血量变更事件

        // 播放受击动画
        PlayHitAnimation();

        // 开启无敌
        isInvulnerable = true;
        invulnerableTimer = hitInvulnerableTime;

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 播放受击动画
    /// </summary>
    private void PlayHitAnimation()
    {
        if (anim != null)
        {
            // 触发受击动画（需在Animator中创建"IsHit"布尔参数）
            anim.SetTrigger("Hit"); // 推荐用Trigger，自动重置；也可用SetBool("IsHit", true)后延迟重置
            // 可选：暂停移动/追击，受击时硬直
            EnemyPatrol patrol = GetComponent<EnemyPatrol>();
            if (patrol != null)
                StartCoroutine(HitStunCoroutine(patrol));
        }
    }

    /// <summary>
    /// 受击硬直协程（可选）
    /// </summary>
    private IEnumerator HitStunCoroutine(EnemyPatrol patrol)
    {
        bool wasChasing = patrol.isChasing;
        patrol.isChasing = false; // 暂停追击
        anim.SetBool("IsMove", false);
        anim.SetBool("IsRun", false);

        yield return new WaitForSeconds(0.2f); // 硬直0.2秒

        if (wasChasing)
            patrol.isChasing = true; // 恢复追击
    }

    /// <summary>
    /// 死亡逻辑
    /// </summary>
    private void Die()
    {
        // 播放死亡动画（需在Animator中创建"IsDead"布尔参数）
        if (anim != null)
        {
            anim.SetBool("IsDead", true);
            anim.SetBool("IsMove", false);
            anim.SetBool("IsRun", false);
            anim.SetBool("IsAttack", false);
        }

        // 禁用所有核心逻辑脚本
        GetComponent<EnemyPatrol>().enabled = false;
        GetComponent<EnemyChase>().enabled = false;
        GetComponent<EnemySight>().enabled = false;
        GetComponent<EnermyAttack>().enabled = false;

        // 触发死亡事件（用于计分、清理等）
        OnEnemyDead?.Invoke();

        // 可选：禁用碰撞体/刚体
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.zero;

        // 可选：延迟销毁物体（等死亡动画播放完）
        Destroy(gameObject, 2f);
    }

    // 可选：重置敌人状态（用于复活/测试）
    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        isInvulnerable = false;
        invulnerableTimer = 0;
        if (anim != null)
        {
            anim.SetBool("IsDead", false);
            anim.ResetTrigger("Hit");
        }
        GetComponent<EnemyPatrol>().enabled = true;
        GetComponent<EnemyChase>().enabled = true;
        GetComponent<EnemySight>().enabled = true;
        GetComponent<EnermyAttack>().enabled = true;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;
    }
}