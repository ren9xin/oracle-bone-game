using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 结局场景的暗号拼字 UI：
//   - 玩家点击下方 4 个字按钮（按任意顺序）→ 字依次填入上方 4 个空槽
//   - 4 槽填满后，"读出口令" 按钮亮起
//   - 点击 "读出口令" → 弹小明读字面板，文字按玩家拼的顺序展示
//   - 3 秒后 / 点 "回家" 按钮 → 跳到 开始界面
//
// 字 sprite 取自 暗号道具系统.Instance.暗号道具集[i].itemImage（演化到当前阶段，结局时通常是唐楷）。
// 任意顺序都算通过——用户的明确诉求是"不预定，玩家自己拼"。
public class 暗号拼字 : MonoBehaviour
{
    [Header("UI 引用 - 字按钮 / 空槽 / 按钮")]
    [Tooltip("4 个字按钮的父容器（水平排列）。")]
    public RectTransform 字按钮容器;

    [Tooltip("4 个空槽的父容器（水平排列）。")]
    public RectTransform 空槽容器;

    [Tooltip("「读出口令」按钮。")]
    public Button 读出按钮;

    [Tooltip("小明读字面板，初始隐藏。")]
    public GameObject 小明读字面板;

    [Tooltip("小明读字面板上的文本，runtime 写入拼好的暗号。")]
    public TextMeshProUGUI 小明读字文本;

    [Tooltip("回家按钮（在小明读字面板上）。")]
    public Button 回家按钮;

    [Header("配置")]
    [Tooltip("拼字成功后跳转的场景。默认是「结局」——结局.scene 内由 结局触发器 自动播主角自白。")]
    public string 回家场景 = "结局";
    public float 自动回家延迟 = 3f;
    public string 读字模板 = "小明：「{0}」……\n这是回家的咒语！家门打开了！";

    [Header("UI 样式")]
    public Sprite 槽位底图;          // 可选：空槽显示的底图
    public Sprite 按钮底图;          // 可选：字按钮的底图
    public TMP_FontAsset 中文字体;

    private List<物品> _道具集 = new List<物品>();
    private List<Image> _槽位列表 = new List<Image>();
    private List<Button> _字按钮列表 = new List<Button>();
    private List<物品> _已选序列 = new List<物品>();
    private bool _应急模式 = false;   // 道具来源全部失败时仍允许"读出口令"直接回家

    void Start()
    {
        if (小明读字面板 != null) 小明读字面板.SetActive(false);
        if (读出按钮 != null)
        {
            读出按钮.interactable = false;
            读出按钮.onClick.RemoveAllListeners();
            读出按钮.onClick.AddListener(读出口令);
        }
        if (回家按钮 != null)
        {
            回家按钮.onClick.RemoveAllListeners();
            回家按钮.onClick.AddListener(回家);
        }

        加载道具集();

        if (_道具集.Count == 0)
        {
            // 应急：连背包里都没暗号道具（直接 Play 结局.scene、存档错位等场景）。
            // 仍让玩家能点 "读出口令" → 回家，避免卡死。
            Debug.LogWarning("暗号拼字：道具来源全部为空，启用应急模式 — 读出按钮直接可点。");
            _应急模式 = true;
            if (读出按钮 != null) 读出按钮.interactable = true;
        }

        创建空槽(_道具集.Count);
        创建字按钮(_道具集);
    }

    // 道具来源优先级（用户明确诉求："暗号拼字启动了就一定要能回家"）：
    //   1. 暗号道具系统.Instance.暗号道具集  —— 正常流程
    //   2. 扫 InventoryManager.instance.myBag.itemList，找带 字形演化图 的物品  —— 暗号道具系统 缺失/空时的兜底
    //   3. 都空 → 应急模式（外层处理）
    void 加载道具集()
    {
        _道具集.Clear();

        // 1) 优先从 暗号道具系统 拿
        if (暗号道具系统.Instance != null && 暗号道具系统.Instance.暗号道具集 != null)
        {
            foreach (var 道具 in 暗号道具系统.Instance.暗号道具集)
                if (道具 != null && !_道具集.Contains(道具)) _道具集.Add(道具);
        }
        if (_道具集.Count > 0) return;

        // 2) Fallback：暗号道具系统 不在 / 道具集为空 → 扫背包
        //    暗号道具的判别：物品 SO 上 图_甲骨/秦篆/唐楷 任一非空（常规物品三阶段全空）
        if (InventoryManager.instance != null
            && InventoryManager.instance.myBag != null
            && InventoryManager.instance.myBag.itemList != null)
        {
            foreach (var 道具 in InventoryManager.instance.myBag.itemList)
            {
                if (道具 == null) continue;
                bool 是暗号 = (道具.图_甲骨 != null) || (道具.图_秦篆 != null) || (道具.图_唐楷 != null);
                if (是暗号 && !_道具集.Contains(道具)) _道具集.Add(道具);
            }
        }

        if (_道具集.Count > 0)
            Debug.Log($"暗号拼字：暗号道具系统 缺失或空，已从背包回退找到 {_道具集.Count} 个暗号道具");
    }

    void 创建空槽(int 数量)
    {
        if (空槽容器 == null) return;
        清空子节点(空槽容器);
        _槽位列表.Clear();
        for (int i = 0; i < 数量; i++)
        {
            var go = new GameObject("槽" + (i + 1));
            go.transform.SetParent(空槽容器, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 180);
            var img = go.AddComponent<Image>();
            if (槽位底图 != null) img.sprite = 槽位底图;
            else img.color = new Color(1, 1, 1, 0.15f); // 半透明白当占位
            img.preserveAspect = true;
            _槽位列表.Add(img);
        }
    }

    void 创建字按钮(List<物品> 道具集)
    {
        if (字按钮容器 == null) return;
        清空子节点(字按钮容器);
        _字按钮列表.Clear();
        for (int i = 0; i < 道具集.Count; i++)
        {
            var 道具 = 道具集[i];
            var go = new GameObject("按钮_" + 道具.itemName);
            go.transform.SetParent(字按钮容器, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 200);
            var img = go.AddComponent<Image>();
            if (按钮底图 != null) img.sprite = 按钮底图;
            else img.color = new Color(0.9f, 0.85f, 0.7f, 0.95f);

            // 字图标（子物体）
            var iconGo = new GameObject("icon");
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.1f, 0.1f);
            iconRt.anchorMax = new Vector2(0.9f, 0.9f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = 道具.itemImage;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            int 捕获索引 = i;
            btn.onClick.AddListener(() => On字按钮点击(_道具集[捕获索引], btn));
            _字按钮列表.Add(btn);
        }
    }

    void On字按钮点击(物品 道具, Button 按钮)
    {
        if (_已选序列.Count >= _槽位列表.Count) return;
        _已选序列.Add(道具);
        // 把对应槽的图标设为此道具
        int 槽索引 = _已选序列.Count - 1;
        if (槽索引 < _槽位列表.Count && _槽位列表[槽索引] != null && 道具 != null)
        {
            _槽位列表[槽索引].sprite = 道具.itemImage;
            _槽位列表[槽索引].color = Color.white;
            _槽位列表[槽索引].preserveAspect = true;
        }
        // 按钮变灰防止重复点
        按钮.interactable = false;

        // 4 槽满，开放读出按钮
        if (读出按钮 != null && _已选序列.Count == _槽位列表.Count)
            读出按钮.interactable = true;
    }

    public void 读出口令()
    {
        // 应急模式：道具来源全空，仍允许走完流程回家
        if (!_应急模式 && _已选序列.Count == 0) return;

        var sb = new System.Text.StringBuilder();
        foreach (var 道具 in _已选序列)
            sb.Append(道具 != null ? 道具.itemName : "?");
        string 拼出 = sb.Length > 0 ? sb.ToString() : "之马开门";  // 应急 fallback
        Debug.Log("暗号拼字：玩家拼出 " + 拼出 + (_应急模式 ? "（应急模式默认咒语）" : ""));

        if (小明读字面板 != null)
        {
            小明读字面板.SetActive(true);
            if (小明读字文本 != null)
                小明读字文本.text = string.Format(读字模板, 拼出);
        }

        Invoke(nameof(回家), Mathf.Max(1f, 自动回家延迟));
    }

    public void 回家()
    {
        CancelInvoke(nameof(回家));
        离开场景();
    }

    void 离开场景()
    {
        if (场景加载器.Instance != null)
            场景加载器.Instance.TransitionToScene(回家场景);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(回家场景);
    }

    void 清空子节点(RectTransform 父)
    {
        for (int i = 父.childCount - 1; i >= 0; i--)
            Destroy(父.GetChild(i).gameObject);
    }
}
