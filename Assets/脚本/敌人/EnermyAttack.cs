using UnityEngine;

public class EnermyAttack : MonoBehaviour
{
    [Header("攻击伤害")]
    public int damage = 10;

    [Header("攻击冷却（秒）")]
    public float attackCooldown = 1f; // 1秒内不会重复受伤

    private float lastAttackTime; // 记录上次攻击时间

    // 2D项目用这个触发方法
    private void OnTriggerStay2D(Collider2D other)
    {
        // 只对标签为"Player"的物体生效
        if (other.CompareTag("Player"))
        {
            // 冷却时间没到，不能再次攻击
            if (Time.time < lastAttackTime + attackCooldown)
                return;

            // 获取主角的受击脚本
            角色脚本 player = other.GetComponent<角色脚本>();
            if (player != null)
            {
                // 让主角受伤
                player.GetHurt(damage);
                lastAttackTime = Time.time; // 记录这次攻击时间
            }
        }
    }
}