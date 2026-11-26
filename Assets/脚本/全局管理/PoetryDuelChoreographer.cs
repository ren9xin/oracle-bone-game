using UnityEngine;

// 长安诗会的对白编排：在合适的时机切换"对话环节"和"答题环节"。
//
// 流程：
//   1. Start: 触发 开场对白（d1 → d2，d2.callbacks 末尾调 PoetryDuelManager.启动关1 → 进入关 1）
//   2. 关 1 胜利 → 触发 学童败北对白（d6 → d7，d7.callbacks 末尾调 PoetryDuelManager.推进到下一关 → 进入关 2）
//   3. 全部通关 → 触发 通关结算对白（d10，结束后什么都不调，HUD 的 全部通关面板 会自动显示）
//
// PoetryDuelManager.自动开始 必须设为 false，否则会和开场对白抢节奏。
public class PoetryDuelChoreographer : MonoBehaviour
{
    [Header("对白 SO 引用")]
    [Tooltip("开场对白：场景加载后立即播放。其链路末尾 callback 必须调用 飞花令系统.启动关1。")]
    public DialogueSO 开场对白;

    [Tooltip("关 1 胜利后播放的对白。其链路末尾 callback 必须调用 飞花令系统.推进到下一关。")]
    public DialogueSO 学童败北对白;

    [Tooltip("全部通关时播放的对白。结束后无需 callback。")]
    public DialogueSO 通关结算对白;

    [Header("延迟（让 Manager.Start 跑完再触发开场对白）")]
    [Tooltip("场景加载后等待多少秒再触发开场对白。0 表示下一帧。")]
    public float 开场延迟秒 = 0.1f;

    void OnEnable()
    {
        if (PoetryDuelManager.Instance != null)
        {
            PoetryDuelManager.Instance.关卡胜利 += On关卡胜利;
            PoetryDuelManager.Instance.全部通关 += On全部通关;
        }
    }

    void OnDisable()
    {
        if (PoetryDuelManager.Instance != null)
        {
            PoetryDuelManager.Instance.关卡胜利 -= On关卡胜利;
            PoetryDuelManager.Instance.全部通关 -= On全部通关;
        }
    }

    void Start()
    {
        // Manager 可能晚一帧才赋值 Instance，等一会儿再触发开场
        Invoke("触发开场", Mathf.Max(0f, 开场延迟秒));
    }

    void 触发开场()
    {
        // 二次确保订阅成功（防 Awake/Start 顺序意外）
        if (PoetryDuelManager.Instance != null)
        {
            PoetryDuelManager.Instance.关卡胜利 -= On关卡胜利;
            PoetryDuelManager.Instance.全部通关 -= On全部通关;
            PoetryDuelManager.Instance.关卡胜利 += On关卡胜利;
            PoetryDuelManager.Instance.全部通关 += On全部通关;
        }

        if (开场对白 != null && 对话管理器.Instance != null)
            对话管理器.Instance.StartDialogue(开场对白);
        else
            Debug.LogWarning("PoetryDuelChoreographer: 开场对白 或 对话管理器.Instance 为空，跳过开场对白。");
    }

    void On关卡胜利(PoetryDuelManager.关卡 关)
    {
        var m = PoetryDuelManager.Instance;
        if (m == null) return;
        // 关 1 胜利 → 播 学童败北 → 链路末尾 callback 进关 2
        // 最后一关（boss）胜利不在此处理 —— 由 全部通关 事件接管
        bool 还有下关 = m.当前关卡索引 < m.关卡列表.Count - 1;
        if (还有下关 && 学童败北对白 != null && 对话管理器.Instance != null)
            对话管理器.Instance.StartDialogue(学童败北对白);
    }

    void On全部通关()
    {
        if (通关结算对白 != null && 对话管理器.Instance != null)
            对话管理器.Instance.StartDialogue(通关结算对白);
    }
}
