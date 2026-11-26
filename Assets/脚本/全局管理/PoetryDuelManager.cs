using System;
using System.Collections.Generic;
using UnityEngine;

// 长安诗会·飞花令管理器（场景内单例，不跨场景）。
// 职责：
//   1) 读取上章 RescueScore，决定玩家初始生命与是否启用"出示拓本"特殊选项
//   2) 加载关卡题库（学童 / 李白），逐题抽题
//   3) 接收 HUD 的点击答案，判对错，扣双方生命
//   4) 关卡推进、通关时把唐字加入字典
//   5) 通过事件广播状态变化（HUD 订阅）
//
// 关卡推进规则：
//   - 题数：关 1=5 题、关 2=7 题（默认值，可在 Inspector 改）。
//   - 答对：对手 -1；答错：玩家 -1。
//   - 任一方生命 ≤ 0 → 关卡结束。
//   - 玩家败 → 触发 关卡失败 事件（HUD 弹重玩按钮）；玩家胜 → 推进到下一关，或全通关。
public class PoetryDuelManager : MonoBehaviour
{
    public static PoetryDuelManager Instance;

    [Serializable]
    public class 关卡
    {
        public string 关卡名;                              // 例："旅店学童"
        public string 对手名;                              // HUD 显示
        public Sprite 对手立绘;                            // HUD 显示
        public List<PoetryQuestion> 题库 = new List<PoetryQuestion>();
        [Tooltip("该关卡要出几题（从题库随机抽，不重复）。")]
        public int 出题数 = 5;
        [Tooltip("对手初始生命。")]
        public int 对手生命 = 3;
        [Tooltip("Boss 关：从玩家字典里再抽 N 个字作为额外主题（动态生成题面，候选诗句仍来自题库匹配）。")]
        public bool 启用字典动态主题 = false;
    }

    [Header("关卡配置")]
    public List<关卡> 关卡列表 = new List<关卡>();

    [Header("玩家初始生命（无 RescueScore 时的默认值）")]
    public int 默认初始生命 = 3;
    [Tooltip("RescueScore.救成功数 ≥ 该阈值时，给玩家额外生命 + 解锁拓本特殊选项。")]
    public int 救成功阈值 = 3;
    public int 高救成功生命 = 4;

    [Header("启动方式")]
    [Tooltip("Start() 时是否自动进入第一关。若 false，需外部（对白结束 callback / 编排脚本）调用 启动关1()。")]
    public bool 自动开始 = true;

    [Header("唐字奖励池（按主题字索引）")]
    [Tooltip("答对一题就根据当前题主题字从这个列表里查 SO 收进字典；列表里没有的主题字答对了也不给字（仅扣对手生命）。")]
    public List<甲骨字> 通关奖励唐字 = new List<甲骨字>();

    // ---- 运行期状态 ----
    public int 玩家生命 { get; private set; }
    public int 玩家最大生命 { get; private set; }
    public int 对手生命 { get; private set; }
    public int 对手最大生命 { get; private set; }
    public int 当前关卡索引 { get; private set; } = -1;
    public PoetryQuestion 当前题 { get; private set; }
    public bool 拓本特殊可用 { get; private set; }      // 救成功数高时，本次 boss 关可用一次"出示拓本"直接答对
    public bool 已使用拓本 { get; private set; }

    // 当前关卡的剩余题（已洗牌）
    private readonly List<PoetryQuestion> _剩余题 = new List<PoetryQuestion>();
    private 关卡 _当前关卡;

    // ---- 事件 ----
    public event Action<PoetryQuestion> 题目刷新;        // 新题展示，HUD 刷主题字 + 4 选项
    public event Action 玩家状态变化;                    // 玩家生命变化
    public event Action 对手状态变化;                    // 对手生命变化
    public event Action<bool, string> 答题结果;          // (是否答对, 旁白/吐槽文本)
    public event Action<关卡> 关卡开始;                  // 进入新关卡（HUD 切对手立绘 + 名字）
    public event Action<关卡> 关卡胜利;                  // 玩家击败该关对手
    public event Action 关卡失败;                        // 玩家被打败（需重玩）
    public event Action 全部通关;                        // 所有关卡通关，触发结局对白

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // 读 RescueScore 决定初始生命
        int 起始生命 = 默认初始生命;
        拓本特殊可用 = false;
        if (RescueScore.Instance != null)
        {
            if (RescueScore.Instance.救成功数 >= 救成功阈值)
            {
                起始生命 = 高救成功生命;
                拓本特殊可用 = true;
            }
        }
        玩家最大生命 = 起始生命;
        玩家生命 = 起始生命;
        已使用拓本 = false;
        玩家状态变化?.Invoke();

        // 自动进入第一关（若关闭，由外部 SendMessage 启动关1 触发）
        if (自动开始 && 关卡列表 != null && 关卡列表.Count > 0) 进入关卡(0);
    }

    // 供 DialogueSO.callback 通过 SendMessage 调用（无参数）
    public void 启动关1()
    {
        if (关卡列表 != null && 关卡列表.Count > 0 && 当前关卡索引 < 0) 进入关卡(0);
    }

    // ------------------ 关卡推进 ------------------

    public void 进入关卡(int 索引)
    {
        if (关卡列表 == null || 索引 < 0 || 索引 >= 关卡列表.Count) return;
        当前关卡索引 = 索引;
        _当前关卡 = 关卡列表[索引];
        对手最大生命 = Mathf.Max(1, _当前关卡.对手生命);
        对手生命 = 对手最大生命;
        对手状态变化?.Invoke();

        // 准备本关题库：保证每个主题字至少出 1 题，剩余名额从池子里随机补。
        // 这样关 1（4 主题，出 5 题）一定 4 主题各 1 + 1 随机，关 2（5 主题，出 7 题）5 各 1 + 2 随机。
        _剩余题.Clear();
        if (_当前关卡.题库 != null)
        {
            // 按主题字分组
            var 按主题 = new Dictionary<string, List<PoetryQuestion>>();
            foreach (var q in _当前关卡.题库)
            {
                if (q == null) continue;
                string k = q.主题字 ?? "";
                if (!按主题.ContainsKey(k)) 按主题[k] = new List<PoetryQuestion>();
                按主题[k].Add(q);
            }
            // 每组内洗牌
            foreach (var lst in 按主题.Values) 洗牌(lst);

            // 每个主题先抽 1 题进"必出"，剩余进"备用"
            var 必出 = new List<PoetryQuestion>();
            var 备用 = new List<PoetryQuestion>();
            foreach (var kv in 按主题)
            {
                if (kv.Value.Count == 0) continue;
                必出.Add(kv.Value[0]);
                for (int i = 1; i < kv.Value.Count; i++) 备用.Add(kv.Value[i]);
            }
            洗牌(必出);
            洗牌(备用);

            int 数量 = Mathf.Min(_当前关卡.出题数, 必出.Count + 备用.Count);
            // 先放必出（每主题 1 题），再用备用补到 出题数
            for (int i = 0; i < 必出.Count && _剩余题.Count < 数量; i++) _剩余题.Add(必出[i]);
            for (int i = 0; i < 备用.Count && _剩余题.Count < 数量; i++) _剩余题.Add(备用[i]);
            // 整体再洗一遍，避免"前几题全是不同主题各 1 题"的明显结构
            洗牌(_剩余题);
        }

        关卡开始?.Invoke(_当前关卡);
        加载下一题();
    }

    public void 加载下一题()
    {
        if (_剩余题.Count == 0)
        {
            // 题出完了但双方都没死 —— 视为玩家胜（题库设计上不会发生，这里兜底）
            处理关卡胜利();
            return;
        }
        当前题 = _剩余题[0];
        _剩余题.RemoveAt(0);
        题目刷新?.Invoke(当前题);
    }

    // ------------------ 答题 ------------------

    // HUD 上点了第 选项索引 个选项（0~3）
    public void 提交答案(int 选项索引)
    {
        if (当前题 == null) return;
        bool 对 = (选项索引 == 当前题.正确索引);
        string 旁白;
        if (对)
        {
            旁白 = "答对！";
            对手扣命(1);
            // 答对收字：按主题字查 唐字奖励池，新字才加（已收集 dedup 静默）
            var 字 = 查找奖励字(当前题.主题字);
            if (字 != null && 字典管理器.Instance != null)
            {
                bool 新字 = 字典管理器.Instance.收集字(字);
            }
        }
        else
        {
            // 错答：告诉玩家这一句空格里其实该填什么
            string 实际挖空 = "";
            if (当前题.选项挖空字 != null && 选项索引 >= 0 && 选项索引 < 当前题.选项挖空字.Length)
                实际挖空 = 当前题.选项挖空字[选项索引];
            if (!string.IsNullOrEmpty(实际挖空))
                旁白 = "那句空格应填" + 实际挖空 + "字";
            else
                旁白 = "可惜，出自 " + (string.IsNullOrEmpty(当前题.出处) ? "诗中" : 当前题.出处);
            玩家扣命(1);
        }
        答题结果?.Invoke(对, 旁白);
        if (!检查关卡结束()) 加载下一题();
    }

    private 甲骨字 查找奖励字(string 现代字)
    {
        if (通关奖励唐字 == null || string.IsNullOrEmpty(现代字)) return null;
        foreach (var 字 in 通关奖励唐字)
            if (字 != null && 字.现代字 == 现代字) return 字;
        return null;
    }

    private static void 洗牌<T>(IList<T> list)
    {
        if (list == null) return;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var t = list[i]; list[i] = list[j]; list[j] = t;
        }
    }

    // boss 关特殊：出示拓本（高救成功时可用一次），直接判为答对
    public void 使用拓本()
    {
        if (!拓本特殊可用 || 已使用拓本) return;
        if (当前题 == null) return;
        已使用拓本 = true;
        对手扣命(1);
        答题结果?.Invoke(true, "拓本上的诗句给了你启发——这一题不必猜了。");
        if (!检查关卡结束()) 加载下一题();
    }

    void 玩家扣命(int 量)
    {
        玩家生命 = Mathf.Max(0, 玩家生命 - 量);
        玩家状态变化?.Invoke();
    }

    void 对手扣命(int 量)
    {
        对手生命 = Mathf.Max(0, 对手生命 - 量);
        对手状态变化?.Invoke();
    }

    bool 检查关卡结束()
    {
        if (玩家生命 <= 0)
        {
            关卡失败?.Invoke();
            return true;
        }
        if (对手生命 <= 0)
        {
            处理关卡胜利();
            return true;
        }
        return false;
    }

    void 处理关卡胜利()
    {
        关卡胜利?.Invoke(_当前关卡);
        int 下一索引 = 当前关卡索引 + 1;
        if (下一索引 < 关卡列表.Count)
        {
            // 进入下一关延后让 HUD 播完对白；HUD/Choreographer 调 推进到下一关
            return;
        }
        // 全部通关 —— 字已在每次答对时收过，这里不再批发
        全部通关?.Invoke();
    }

    // 玩家失败后重玩本关
    public void 重玩当前关卡()
    {
        玩家生命 = 玩家最大生命;
        玩家状态变化?.Invoke();
        if (当前关卡索引 >= 0) 进入关卡(当前关卡索引);
    }

    // 供 HUD 在胜利对白结束时调用：进入下一关
    public void 推进到下一关()
    {
        int 下一 = 当前关卡索引 + 1;
        if (下一 < 关卡列表.Count) 进入关卡(下一);
    }
}
