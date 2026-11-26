using UnityEngine;

// 跨章节"暗号道具"系统：
//   - 王宫剧情末尾把 4 个甲骨文字形道具（之/马/开/门）放进玩家背包。
//   - 每个章节末尾回到次元空间时，由 次元空间Hub 调用 演化到() 推进字形（甲骨→秦篆→唐楷）。
//   - 启动时（如读档进入次元空间）按 GameManager.unlockedGateIndex 自动同步字形阶段，避免存档错位。
//
// 挂载方式：场景中一个空 GameObject 命名为 "剧情系统"，挂本组件，Inspector 拖入 4 个甲骨文-* 物品 SO。
// 单例 + DontDestroyOnLoad 保证跨场景。
public class 暗号道具系统 : MonoBehaviour
{
    public static 暗号道具系统 Instance;

    [Tooltip("4 个暗号道具的 物品 SO 引用（拖 甲骨文-之/马/开/门.asset）。")]
    public 物品[] 暗号道具集 = new 物品[0];

    [Tooltip("赋予道具完毕后自动传送到的场景名。留空则不传送（仅赋予）。")]
    public string 赋予后传送场景 = "次元空间";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 启动 / 读档恢复时：从 GameManager.暗号字形阶段 字段恢复字形（存档已写回该字段）
        应用阶段(当前应有阶段());
    }

    // 字形阶段权威源：GameManager.暗号字形阶段（存档写回）。
    // 老存档没这个字段（默认 0=甲骨），所以新存档前的玩家会先从甲骨开始，之后由 次元空间Hub 推进。
    public 物品.字形阶段 当前应有阶段()
    {
        if (GameManager.Instance == null) return 物品.字形阶段.甲骨;
        return (物品.字形阶段)Mathf.Clamp(GameManager.Instance.暗号字形阶段, 0, 2);
    }

    void 应用阶段(物品.字形阶段 阶段)
    {
        if (暗号道具集 == null) return;
        foreach (var 道具 in 暗号道具集)
            if (道具 != null) 道具.应用字形阶段(阶段);
    }

    // ============================================================
    // 王宫对白 callback：把 4 个甲骨碎片加入背包
    // ============================================================
    public void 赋予暗号道具()
    {
        if (暗号道具集 == null || 暗号道具集.Length == 0)
        {
            Debug.LogWarning("暗号道具系统.赋予暗号道具：暗号道具集为空，跳过");
            return;
        }
        // 确保字形先回到甲骨阶段（防 Editor Play 时 SO 残留）
        应用阶段(物品.字形阶段.甲骨);
        foreach (var 道具 in 暗号道具集)
            if (道具 != null) InventoryManager.添加物品(道具);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.是否获得甲骨文 = true;
            GameManager.Instance.暗号字形阶段 = 0;
        }
        Debug.Log($"✅ 暗号道具系统：已赋予 {暗号道具集.Length} 个甲骨碎片");
    }

    // ============================================================
    // 王宫对白 callback：UnlockNextGate + 回到次元空间
    // ============================================================
    public void 传送回次元空间()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockNextGate();
            // 王宫穿越后，血条UI / 金币UI 永久隐藏（穿越主角不再有现代 HUD）
            GameManager.Instance.血金UI隐藏 = true;
            GameManager.Instance.刷新血金UI();
        }
        if (!string.IsNullOrEmpty(赋予后传送场景) && 场景加载器.Instance != null)
            场景加载器.Instance.TransitionToScene(赋予后传送场景);
        else if (!string.IsNullOrEmpty(赋予后传送场景))
            UnityEngine.SceneManagement.SceneManager.LoadScene(赋予后传送场景);
    }

    // ============================================================
    // 次元空间Hub 调用：演化道具字形到指定阶段
    // ============================================================
    public void 演化到(物品.字形阶段 阶段)
    {
        应用阶段(阶段);
        InventoryManager.强制刷新UI();
        Debug.Log($"✅ 暗号道具：字形演化到 {阶段}");
    }

    // 便捷查询：暗号道具是否已经全部到玩家背包了
    public bool 道具已全部赋予()
    {
        if (暗号道具集 == null || InventoryManager.instance == null
            || InventoryManager.instance.myBag == null) return false;
        var list = InventoryManager.instance.myBag.itemList;
        foreach (var 道具 in 暗号道具集)
            if (道具 != null && !list.Contains(道具)) return false;
        return true;
    }
}
