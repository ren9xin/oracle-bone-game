using System;
using TMPro;
using UnityEngine;

// 甲骨字字典管理器：单例 + DontDestroyOnLoad。
// 职责：收集字、刷新字典面板、显示选中字的释义、对外提供查询。
// 通过外部调用 收集字() 触发新增；按 切换键（默认 J）显示/隐藏字典面板。
public class 字典管理器 : MonoBehaviour
{
    public static 字典管理器 Instance;

    [Header("数据")]
    public 甲骨字典 我的字典;      // Inspector 拖入运行期字典 SO

    [Header("UI 引用")]
    public GameObject 字典面板;    // 字典面板根节点（默认隐藏）
    public Transform 槽容器;       // 槽的父物体（GridLayoutGroup）
    public 字典槽 槽预制体;
    public TextMeshProUGUI 释义文本;
    public TextMeshProUGUI 现代字文本;

    [Header("快捷键")]
    public KeyCode 切换键 = KeyCode.J;

    public 甲骨字 当前选中 { get; private set; }

    // 外部可订阅，剧情/解谜逻辑用
    public event Action<甲骨字> 字被收集;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (字典面板 != null) 字典面板.SetActive(false);
    }

    void Update()
    {
        if (对话管理器.Instance != null && 对话管理器.Instance.isInDialogue) return;
        if (Input.GetKeyDown(切换键)) 切换字典面板();
    }

    public void 切换字典面板()
    {
        if (字典面板 == null) return;
        bool 即将打开 = !字典面板.activeSelf;
        字典面板.SetActive(即将打开);
        if (即将打开) 刷新();
    }

    // 收集一个字。返回 true 表示是新字，false 表示已在字典里。
    public bool 收集字(甲骨字 字)
    {
        if (字 == null || 我的字典 == null) return false;
        if (我的字典.已收集字.Contains(字)) return false;
        我的字典.已收集字.Add(字);
        字被收集?.Invoke(字);
        if (字典面板 != null && 字典面板.activeSelf) 刷新();
        return true;
    }

    public bool 已收集(甲骨字 字)
    {
        return 字 != null && 我的字典 != null && 我的字典.已收集字.Contains(字);
    }

    public bool 已收集(string 现代字)
    {
        return 查找(现代字) != null;
    }

    // 按现代字查找已收集的甲骨字（解谜场景用）
    public 甲骨字 查找(string 现代字)
    {
        if (我的字典 == null) return null;
        foreach (var 字 in 我的字典.已收集字)
            if (字 != null && 字.现代字 == 现代字) return 字;
        return null;
    }

    public int 已收集数量 => 我的字典 != null ? 我的字典.已收集字.Count : 0;

    public void 选中(甲骨字 字)
    {
        当前选中 = 字;
        if (字 == null)
        {
            if (释义文本 != null) 释义文本.text = "";
            if (现代字文本 != null) 现代字文本.text = "";
            return;
        }
        if (释义文本 != null) 释义文本.text = 字.释义;
        if (现代字文本 != null) 现代字文本.text = 字.现代字;
    }

    public void 刷新()
    {
        if (槽容器 == null || 槽预制体 == null || 我的字典 == null) return;
        for (int i = 槽容器.childCount - 1; i >= 0; i--)
            Destroy(槽容器.GetChild(i).gameObject);
        foreach (var 字 in 我的字典.已收集字)
        {
            if (字 == null) continue;
            字典槽 槽 = Instantiate(槽预制体, 槽容器);
            槽.绑定(字);
        }
    }
}
