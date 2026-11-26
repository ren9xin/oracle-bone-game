using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 长安诗会·飞花令 HUD。订阅 PoetryDuelManager 事件，刷主题字 / 4 选项按钮 / 双方生命 / 灵蝉吐槽。
// 在场景里把所有引用拖上；4 个选项按钮的 onClick 用 Inspector 持久化绑定到 选项0..选项3 方法。
public class PoetryDuelHUD : MonoBehaviour
{
    [Header("主题字 + 题面")]
    public TextMeshProUGUI 主题字文本;        // 大字「花」
    public TextMeshProUGUI 题面文本;          // 例："下列哪句诗中有「花」字？"
    public string 题面模板 = "下列哪句诗的 〇 处应填 “{0}” 字？";

    [Tooltip("候选诗句中挖空字的显示符号。默认 〇。")]
    public string 挖空符号 = "〇";

    [Header("4 个候选按钮")]
    public Button[] 选项按钮 = new Button[4];
    public TextMeshProUGUI[] 选项文本 = new TextMeshProUGUI[4];

    [Header("生命条")]
    public TextMeshProUGUI 玩家生命文本;
    public TextMeshProUGUI 对手生命文本;
    public string 玩家生命模板 = "小明 ♥ {0}/{1}";
    public string 对手生命模板 = "{2} ♥ {0}/{1}";

    [Header("对手立绘 + 名字")]
    public Image 对手立绘图;
    public TextMeshProUGUI 对手名文本;

    [Header("玩家立绘")]
    public Image 玩家立绘图;

    [Header("灵蝉吐槽 / 旁白")]
    public TextMeshProUGUI 旁白文本;
    [Tooltip("旁白文本显示后多少秒自动清空（≤0 不自动清）。")]
    public float 旁白持续秒 = 2.5f;

    [Header("特殊：出示拓本按钮（boss 关 + 救成功数高 时显示）")]
    public Button 拓本按钮;

    [Header("失败 / 胜利面板")]
    public GameObject 关卡失败面板;
    public GameObject 全部通关面板;
    public Button 重玩按钮;
    public Button 下一关按钮;

    [Header("字体（避免 TMP 中文豆腐块）")]
    public TMP_FontAsset 中文字体;

    private float _旁白计时;

    void Start()
    {
        var m = PoetryDuelManager.Instance;
        if (m != null)
        {
            m.题目刷新 += OnQuestionLoaded;
            m.玩家状态变化 += OnPlayerChanged;
            m.对手状态变化 += OnEnemyChanged;
            m.答题结果 += OnAnswerResult;
            m.关卡开始 += OnLevelStart;
            m.关卡胜利 += OnLevelWin;
            m.关卡失败 += OnLevelLose;
            m.全部通关 += OnAllClear;
            // 初次同步（覆盖订阅前 Manager.Start 已经 fire 过的题目刷新事件）
            OnPlayerChanged();
            OnEnemyChanged();
            if (m.当前题 != null) OnQuestionLoaded(m.当前题);
            else 清空题面();
        }

        if (关卡失败面板 != null) 关卡失败面板.SetActive(false);
        if (全部通关面板 != null) 全部通关面板.SetActive(false);
        if (拓本按钮 != null) 拓本按钮.gameObject.SetActive(false);

        // 字体统一刷一遍（HUD 上挂的 TMP 引用都设字体）
        if (中文字体 != null) 应用字体();
    }

    void OnDestroy()
    {
        var m = PoetryDuelManager.Instance;
        if (m == null) return;
        m.题目刷新 -= OnQuestionLoaded;
        m.玩家状态变化 -= OnPlayerChanged;
        m.对手状态变化 -= OnEnemyChanged;
        m.答题结果 -= OnAnswerResult;
        m.关卡开始 -= OnLevelStart;
        m.关卡胜利 -= OnLevelWin;
        m.关卡失败 -= OnLevelLose;
        m.全部通关 -= OnAllClear;
    }

    private bool _上次在对话 = false;
    private bool _等待显示通关面板 = false;
    private bool _全部通关已触发 = false;
    void Update()
    {
        if (_旁白计时 > 0f)
        {
            _旁白计时 -= Time.deltaTime;
            if (_旁白计时 <= 0f && 旁白文本 != null) 旁白文本.text = "";
        }

        bool 在对话 = 对话管理器.Instance != null && 对话管理器.Instance.isInDialogue;

        if (_全部通关已触发)
        {
            // 通关后不再恢复 答题 HUD；只等通关对白结束 → 显示通关面板
            if (_等待显示通关面板 && !在对话)
            {
                if (全部通关面板 != null) 全部通关面板.SetActive(true);
                _等待显示通关面板 = false;
            }
        }
        else if (在对话 != _上次在对话)
        {
            设置答题UI可见(!在对话);
            _上次在对话 = 在对话;
        }
    }

    void 设置答题UI可见(bool 可见)
    {
        // 题面 / 选项 / 旁白 / 拓本
        if (主题字文本 != null) 主题字文本.gameObject.SetActive(可见);
        if (题面文本 != null) 题面文本.gameObject.SetActive(可见);
        if (旁白文本 != null) 旁白文本.gameObject.SetActive(可见);
        for (int i = 0; i < 选项按钮.Length; i++)
        {
            if (选项按钮[i] != null)
            {
                选项按钮[i].gameObject.SetActive(可见);
                选项按钮[i].interactable = 可见;
            }
        }
        if (拓本按钮 != null && PoetryDuelManager.Instance != null)
        {
            var m = PoetryDuelManager.Instance;
            bool 显示拓本 = 可见 && m.拓本特殊可用 && !m.已使用拓本
                          && m.当前关卡索引 == m.关卡列表.Count - 1
                          && m.当前题 != null;
            拓本按钮.gameObject.SetActive(显示拓本);
        }

        // 立绘 / 生命条 / 对手名 —— 跟答题一个道理，对话期间也要让位
        if (对手立绘图 != null) 对手立绘图.gameObject.SetActive(可见);
        if (玩家立绘图 != null) 玩家立绘图.gameObject.SetActive(可见);
        if (玩家生命文本 != null) 玩家生命文本.gameObject.SetActive(可见);
        if (对手生命文本 != null) 对手生命文本.gameObject.SetActive(可见);
        if (对手名文本 != null) 对手名文本.gameObject.SetActive(可见);
    }

    void 清空题面()
    {
        if (主题字文本 != null) 主题字文本.text = "";
        if (题面文本 != null) 题面文本.text = "";
        for (int i = 0; i < 选项文本.Length; i++)
            if (选项文本[i] != null) 选项文本[i].text = "";
    }

    // 把 line 中 blank 第一次出现的位置替换为 挖空符号；找不到则原样返回（兜底）。
    string 挖空显示(string line, string blank)
    {
        if (string.IsNullOrEmpty(line)) return "";
        if (string.IsNullOrEmpty(blank)) return line;
        int idx = line.IndexOf(blank);
        if (idx < 0) return line;
        string sym = string.IsNullOrEmpty(挖空符号) ? "〇" : 挖空符号;
        return line.Substring(0, idx) + sym + line.Substring(idx + blank.Length);
    }

    // ---------------- 按钮点击入口（Inspector 持久化绑定到这 4 个方法）----------------

    public void 选项0() { Submit(0); }
    public void 选项1() { Submit(1); }
    public void 选项2() { Submit(2); }
    public void 选项3() { Submit(3); }

    void Submit(int 索引)
    {
        if (PoetryDuelManager.Instance != null) PoetryDuelManager.Instance.提交答案(索引);
    }

    public void 点击拓本()
    {
        if (PoetryDuelManager.Instance != null) PoetryDuelManager.Instance.使用拓本();
        if (拓本按钮 != null) 拓本按钮.gameObject.SetActive(false);
    }

    public void 点击重玩()
    {
        if (关卡失败面板 != null) 关卡失败面板.SetActive(false);
        if (PoetryDuelManager.Instance != null) PoetryDuelManager.Instance.重玩当前关卡();
    }

    public void 点击下一关()
    {
        if (下一关按钮 != null) 下一关按钮.gameObject.SetActive(false);
        if (PoetryDuelManager.Instance != null) PoetryDuelManager.Instance.推进到下一关();
    }

    // 全部通关面板 上点击 → 先回次元空间（演化 秦篆→唐楷 + 走回家门），由回家门跳结局
    public void 点击结束()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnlockNextGate();
        if (场景加载器.Instance != null)
            场景加载器.Instance.TransitionToScene("次元空间");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("次元空间");
    }

    // ---------------- 事件响应 ----------------

    void OnQuestionLoaded(PoetryQuestion 题)
    {
        if (题 == null) return;
        if (主题字文本 != null) 主题字文本.text = 题.主题字 ?? "";
        if (题面文本 != null) 题面文本.text = string.Format(题面模板, 题.主题字 ?? "");
        for (int i = 0; i < 选项文本.Length; i++)
        {
            if (选项文本[i] == null) continue;
            string 原句 = (题.候选诗句 != null && i < 题.候选诗句.Length) ? 题.候选诗句[i] : "";
            string 挖 = (题.选项挖空字 != null && i < 题.选项挖空字.Length) ? 题.选项挖空字[i] : "";
            选项文本[i].text = 挖空显示(原句, 挖);
        }
        // 开放所有选项按钮
        for (int i = 0; i < 选项按钮.Length; i++)
            if (选项按钮[i] != null) 选项按钮[i].interactable = true;

        // 拓本按钮：boss 关（按 manager 标志） + 还没用过
        if (拓本按钮 != null)
        {
            var m = PoetryDuelManager.Instance;
            bool 显示 = m != null && m.拓本特殊可用 && !m.已使用拓本 && m.当前关卡索引 == m.关卡列表.Count - 1;
            拓本按钮.gameObject.SetActive(显示);
        }
    }

    void OnPlayerChanged()
    {
        if (玩家生命文本 == null || PoetryDuelManager.Instance == null) return;
        var m = PoetryDuelManager.Instance;
        玩家生命文本.text = string.Format(玩家生命模板, m.玩家生命, m.玩家最大生命);
    }

    void OnEnemyChanged()
    {
        if (对手生命文本 == null || PoetryDuelManager.Instance == null) return;
        var m = PoetryDuelManager.Instance;
        string 名 = (m.当前关卡索引 >= 0 && m.当前关卡索引 < m.关卡列表.Count)
            ? m.关卡列表[m.当前关卡索引].对手名 : "对手";
        对手生命文本.text = string.Format(对手生命模板, m.对手生命, m.对手最大生命, 名);
    }

    void OnAnswerResult(bool 对, string 旁白)
    {
        if (旁白文本 != null)
        {
            旁白文本.text = 旁白 ?? "";
            _旁白计时 = Mathf.Max(0f, 旁白持续秒);
        }
    }

    void OnLevelStart(PoetryDuelManager.关卡 关)
    {
        if (关 == null) return;
        if (对手立绘图 != null)
        {
            对手立绘图.sprite = 关.对手立绘;
            对手立绘图.enabled = (关.对手立绘 != null);
        }
        if (对手名文本 != null) 对手名文本.text = 关.对手名 ?? "";
        if (关卡失败面板 != null) 关卡失败面板.SetActive(false);
        if (下一关按钮 != null) 下一关按钮.gameObject.SetActive(false);
        OnEnemyChanged();
    }

    void OnLevelWin(PoetryDuelManager.关卡 关)
    {
        // 关卡胜利后 题面 / 选项保持上一题状态；由 PoetryDuelChoreographer 触发对白；对白末尾 callback 进下一关。
        // 不再自动显示"下一关"按钮（防止与对白冲突）。
    }

    void OnLevelLose()
    {
        if (关卡失败面板 != null) 关卡失败面板.SetActive(true);
        // 禁用选项按钮，防止误点
        for (int i = 0; i < 选项按钮.Length; i++)
            if (选项按钮[i] != null) 选项按钮[i].interactable = false;
    }

    void OnAllClear()
    {
        // 通关时立即隐藏 答题 HUD；通关面板推迟到通关对白结束 → Update 中开启
        _全部通关已触发 = true;
        _等待显示通关面板 = true;
        设置答题UI可见(false);
        if (全部通关面板 != null) 全部通关面板.SetActive(false);
    }

    // ---------------- 工具 ----------------

    void 应用字体()
    {
        TextMeshProUGUI[] 所有 = {
            主题字文本, 题面文本, 玩家生命文本, 对手生命文本, 对手名文本, 旁白文本
        };
        foreach (var t in 所有) if (t != null) t.font = 中文字体;
        foreach (var t in 选项文本) if (t != null) t.font = 中文字体;
    }
}
