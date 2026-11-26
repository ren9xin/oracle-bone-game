using UnityEngine;

public class PlayerGravitySetter : MonoBehaviour
{
    [Header("目标重力")]
    public float targetGravity = 0;

    [Header("还原时的正常重力")]
    public float originalGravity = 2.7f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 记录初始重力
        if (rb != null)
            originalGravity = rb.gravityScale;
    }

    /// <summary>
    /// 设置为你指定的重力
    /// </summary>
    public void SetTargetGravity()
    {
        if (rb != null)
            rb.gravityScale = targetGravity;
    }

    /// <summary>
    /// 还原成原来的重力
    /// </summary>
    public void RestoreOriginalGravity()
    {
        if (rb != null)
            rb.gravityScale = originalGravity;
    }
}