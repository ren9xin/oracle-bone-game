using UnityEngine;

// 一卷竹简（可拾取）。
// 是真甲骨拓本=true → F 拾取记一次救成功，关联字会进字典。
// 是真甲骨拓本=false → F 拾取记一次错带（扣命）。
// 火势.cs 也可以调 被火烧毁() 触发烧毁记录 + 销毁 GameObject。
public class BambooScroll : MonoBehaviour
{
    [Header("内容")]
    public bool 是真甲骨拓本;
    public 甲骨字 关联字;          // 真拓本拾起时顺手加进字典

    [Header("交互")]
    public KeyCode 拾取键 = KeyCode.F;

    bool 在范围内;

    void Update()
    {
        if (!在范围内) return;
        if (Input.GetKeyDown(拾取键)) 拾取();
    }

    void 拾取()
    {
        if (RescueScore.Instance != null)
        {
            if (是真甲骨拓本) RescueScore.Instance.记一次救成功(关联字);
            else RescueScore.Instance.记一次错带();
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 在范围内 = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 在范围内 = false;
    }

    // 火势蔓延组件（外部）调用：竹简被火吞了
    public void 被火烧毁()
    {
        if (RescueScore.Instance != null) RescueScore.Instance.记一次烧毁();
        Destroy(gameObject);
    }
}
