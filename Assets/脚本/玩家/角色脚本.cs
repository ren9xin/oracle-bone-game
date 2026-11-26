using UnityEngine;

public class 角色脚本 : MonoBehaviour
{
    [SerializeField] float speed = 3.0f;
    [SerializeField] float jumpForce = 1000f;

    [Header("Sound names (Resources)")]
    [SerializeField] string soundJump = "跳";

    [Header("地面检测")]
    public Transform groundCheckPoint;  // 拖入你脚底的空对象
    public float groundCheckDistance = 0.15f; // 射线长度
    public float offset = 0.3f;// 左右偏移距离（根据你的角色大小微调）

    private Rigidbody2D m_body2d;
    public bool m_grounded = false;
    public int defaultMaxHealth = 100;//最大生命值
    [HideInInspector] public int currentHealth;//当前生命值
    [HideInInspector] public int moneycount;//玩家钱数
    public 血条 healthBar;//玩家血条
    public 金币UI money;//玩家钱

    public int playerID;
    private float lastDamageTime;
    public Animator ani;//角色动画（可能为空）
    Rigidbody2D rbody;
    public bool motion = true;//允许触发动作
    private Vector3 originalScale;// 角色初始缩放

    private bool isJumping; // 动画参数：是否跳跃中（在空中）
    // 对话模式：禁止一切操作
    public bool isInDialogue = false;

    // 在这里标记你的动画有没有 IsJumping等 参数
    [Header("动画参数")]
    public bool hasIsJumpingParameter = false;
    public bool hasGroundParameter = false;
    public bool hasJumpParameter = false;
    public bool hasIsMoveParameter = false;
    public bool hasClimbingParameter = false;

    private float coyoteTime = 0.1f;//土狼时间（跳跃离开平台时用）
    private float coyoteCounter;

    public GameObject deathUI;


    void Start()
    {
        // ========== 1. 刚体初始化（必选组件，加容错） ==========
        rbody = GetComponent<Rigidbody2D>();
        if (rbody == null)
        {
            Debug.LogWarning("角色缺少Rigidbody2D组件，移动/跳跃功能将失效！");
        }

        m_body2d = rbody; // 复用刚体引用，避免重复获取

        // ========== 2. Animator初始化（可选组件，重点容错） ==========
        ani = GetComponent<Animator>();
        if (ani == null)
        {
            Debug.LogWarning("角色缺少Animator组件，动画相关逻辑将跳过（不影响移动）！");
        }

        // 其他初始化逻辑（GameManager 在主流程里由 次元空间.scene 创建；若单独 Play 子场景，Instance 可能为 null）
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.maxHealth == 0)
            {
                GameManager.Instance.SetPlayerData(defaultMaxHealth, 0, defaultMaxHealth);
            }
            currentHealth = GameManager.Instance.currentHealth;
            moneycount = GameManager.Instance.moneyCount;
        }
        else
        {
            Debug.LogWarning("角色脚本.Start: GameManager.Instance 为空（单独 Play 子场景？），用本地默认值。");
            currentHealth = defaultMaxHealth;
            moneycount = 0;
        }

        lastDamageTime = Time.time;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (PlayerInputLock.inputLocked) return;
        // 对话中：只播站立，禁用所有操作
        if (isInDialogue)
        {
            if (ani != null)
            {
                if (hasIsMoveParameter)
                {
                    ani.SetBool("isMove", false);
                }

                if (hasGroundParameter)
                {
                    ani.SetBool("Grounded", true);
                }

                if (hasJumpParameter)
                {
                    ani.SetBool("IsJumping", false);
                }

            }

            if (rbody != null)
                rbody.velocity = new Vector2(0, rbody.velocity.y);

            return;
        }

        // 每帧检测是否在地面（射线）
        m_grounded = CheckGrounded();
        if (m_grounded)
        {
            coyoteCounter = coyoteTime; // 在地面就刷新时间
        }
        else
        {
            coyoteCounter -= Time.deltaTime; // 不在地面就倒计时
        }

        if (motion)//允许角色触发动作
        {
            Move();
            Jump();
        }

        isJumping = !m_grounded && rbody.velocity.y > 0.1f;

        // 传给动画
        if (ani != null)
        {
            if (hasIsJumpingParameter)
            { ani.SetBool("IsJumping", isJumping); }
            if (hasGroundParameter)
                ani.SetBool("Grounded", m_grounded);
            if (!m_grounded)
            {
                // 保持当前动画不切回 idle/run
                if (hasIsMoveParameter)
                    ani.SetBool("isMove", false);
            }
        }

    }

    private void Move()
    {
        // 刚体为空时直接返回，避免空引用报错
        if (rbody == null) return;

        float horizontal = Input.GetAxis("Horizontal");
        if (!m_grounded)
        {
            rbody.velocity = new Vector2(horizontal * speed, rbody.velocity.y);
            return;
        }

        if (Mathf.Abs(horizontal) > 0.1f)
        {
            // ========== 3. 动画逻辑：先判空再使用 ==========
            if (ani != null)
            {
                if (hasIsMoveParameter)
                    ani.SetBool("isMove", true);
            }

            // 转身逻辑（无Animator也能正常执行）
            transform.localScale = new Vector3(
                originalScale.x * (horizontal > 0 ? -1 : 1),
                originalScale.y,
                originalScale.z);

            // 刚体移动逻辑
            Vector2 moveVelocity = rbody.velocity;
            moveVelocity.x = horizontal * speed;
            rbody.velocity = moveVelocity;
        }
        else
        {
            // ========== 4. 动画逻辑：判空修复 ==========
            if (ani != null)
            {
                if (hasIsMoveParameter)
                    ani.SetBool("isMove", false);
            }
            // 停止移动
            rbody.velocity = new Vector2(0, rbody.velocity.y);
        }
    }

    void Jump()
    {
        if (rbody == null) return;


        // 按空格 + 在地面上
        if (Input.GetKeyDown(KeyCode.Space) && coyoteCounter > 0)
        {
            rbody.velocity = new Vector2(rbody.velocity.x, 0); // 重置Y速度
            rbody.AddForce(Vector2.up * jumpForce);
            coyoteCounter = 0; // 跳完立刻清空，防止连跳
            if (ani != null && hasJumpParameter) ani.SetTrigger("Jump");
        }
    }

    public void GetHurt(int Demage)
    {
        currentHealth = Mathf.Max(0, currentHealth - Demage);
        if (ani != null)
        {
            ani.SetTrigger("Hit");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        // 通知全局并刷新UI
        GameManager.Instance.SetPlayerData(currentHealth, moneycount, defaultMaxHealth);
    }

    public void GetMoney(int count)
    {
        moneycount += count;
        GameManager.Instance.SetPlayerData(currentHealth, moneycount, defaultMaxHealth);
    }

    void Die()
    {
        Debug.Log("主角死亡！");
        if (ani != null)
        {
            ani.SetTrigger("IsDead");
            MoveBaned();
        }
    }

    // 死亡动画播放完 自动调用
    public void OnDeadAnimEnd()
    {
        Debug.Log("查找死亡界面");
        // 查找场景中名字叫 死亡界面 的UI物体
        if (deathUI != null)
        {
            deathUI.SetActive(true);
        }
    }
    public void MoveBaned()//禁止玩家动作
    {
        motion = false;
        // 禁止动作时停止移动，判空避免报错
        if (rbody != null)
        {
            rbody.velocity = Vector2.zero;
        }
    }

    public void MoveAxis()//允许玩家触发动作
    {
        motion = true;
    }

    // ==============================================
    // 射线地面检测（真正只认脚下，解决箱子BUG）
    // ==============================================
    private bool CheckGrounded()
    {
        // 爬梯子时直接返回 false，不做射线检测
        爬梯子 ladder = GetComponent<爬梯子>();
        if (ladder != null && ladder.isClimbing)
        {
            return true;
        }
        if (groundCheckPoint == null)
            return false;

        // 计算左右两个射线发射点
        Vector2 centerPos = groundCheckPoint.position;
        Vector2 leftPos = centerPos + new Vector2(-offset, 0);
        Vector2 rightPos = centerPos + new Vector2(offset, 0);

        // 绘制双射线（Scene窗口可见）
        Debug.DrawRay(leftPos, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(rightPos, Vector2.down * groundCheckDistance, Color.red);

        // 发射左右两根射线
        RaycastHit2D hitLeft = Physics2D.Raycast(leftPos, Vector2.down, groundCheckDistance);
        RaycastHit2D hitRight = Physics2D.Raycast(rightPos, Vector2.down, groundCheckDistance);

        // 判断：任意一根碰到 ground 就算落地
        bool groundLeft = hitLeft.collider != null && hitLeft.collider.CompareTag("ground");
        bool groundRight = hitRight.collider != null && hitRight.collider.CompareTag("ground");

        return groundLeft || groundRight;

    }
}