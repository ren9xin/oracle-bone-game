using UnityEngine;

// 挂在场景中"可被收集的甲骨字"GameObject 上。
// 玩家进入触发器范围 + 按 F：把 字 加到字典管理器，然后销毁自身。
// 用法：节点上挂 SpriteRenderer + Collider2D(IsTrigger) + 本脚本，把 字 SO 拖进 Inspector。
public class 拾字 : MonoBehaviour
{
    public 甲骨字 字;
    public KeyCode 拾取键 = KeyCode.F;

    bool 在范围内;

    void Start()
    {
        // 已收集过的字不再出现在场景里
        if (字 != null && 字典管理器.Instance != null && 字典管理器.Instance.已收集(字))
            Destroy(gameObject);
    }

    void Update()
    {
        if (!在范围内) return;
        if (Input.GetKeyDown(拾取键)) 拾取();
    }

    void 拾取()
    {
        if (字 == null || 字典管理器.Instance == null) return;
        bool 新增 = 字典管理器.Instance.收集字(字);
        if (新增) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 在范围内 = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 在范围内 = false;
    }
}
