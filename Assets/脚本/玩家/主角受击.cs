using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth;
    private int currentHealth;
    public Animator anim;
    角色脚本 player;

    void Start()
    {
        player=GetComponent<角色脚本>();
        anim = GetComponent<Animator>();
    }

    // 士兵攻击脚本会调用这个方法
    public void TakeDamage(int damage)
    {
        if (player == null) return;
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log("主角受伤！剩余血量：" + currentHealth);

        // 播放受击动画（Animator里需要有"Hit"这个Trigger参数）
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("主角死亡！");
        if (anim != null)
        {
            anim.SetBool("IsDead",true);
        }
    }
}