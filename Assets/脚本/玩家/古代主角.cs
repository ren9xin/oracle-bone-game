using UnityEngine;

public class 古代主角补充版 : MonoBehaviour
{
    private Animator anim;
    private 角色脚本 player;

    private bool isFacingRight = true;
    private bool isDead = false;

    // 攻击冷却：动画没结束前不能再攻击
    private bool canAttack = true;
    [Header("攻击冷却时间（和动画时长一致）")]
    public float attackCD = 0.66f;
    public float heavyAttackCD = 1f;
    public bool allowAttack=false;//是否可以攻击

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<角色脚本>();
    }

    void Update()
    {
        if (PlayerInputLock.inputLocked) return;
        // 对话中：禁用所有攻击、翻转、动画
        if (player.isInDialogue)
        {
            if (anim != null)
            {
                anim.SetFloat("Speed", 0);
            }
            return;
        }
        if (isDead || !player.motion) return;

        if (allowAttack) 
        { 
            // 普通攻击
            if (Input.GetMouseButtonDown(0) && canAttack)
            {
             Attack();
            }

            // 重击
            if (Input.GetMouseButtonDown(1) && canAttack)
            {
                HeavyAttack();
            }
        }

      

        // 死亡 K
        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
        }

        AutoFlip();
    }

    // 普通攻击
    void Attack()
    {
        canAttack = false;
        if (anim != null) anim.SetTrigger("Attack");
        Invoke(nameof(ResetAttack), attackCD);
    }

    // 重击
    void HeavyAttack()
    {
        canAttack = false;
        if (anim != null) anim.SetTrigger("HeavyAttack");
        Invoke(nameof(ResetAttack), heavyAttackCD);
    }

    // 冷却结束，允许再次攻击
    void ResetAttack()
    {
        canAttack = true;
    }

    // 自动翻转
    void AutoFlip()
    {
        float horizontal = Input.GetAxis("Horizontal");

        if ((isFacingRight && horizontal < 0) || (!isFacingRight && horizontal > 0))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(horizontal));
        }
    }

    // 受击
    /*public void TakeDamage()
    {
        if (isDead) return;
        if (anim != null) anim.SetTrigger("Hit");
        player.GetHurt();
    }*/

    // 死亡
    public void Die()
    {
        isDead = true;
        canAttack = false;
        if (anim != null) anim.SetTrigger("IsDead");
        player.MoveBaned();
    }

    public void CanAttack()
    {
        allowAttack = true;
    }
}