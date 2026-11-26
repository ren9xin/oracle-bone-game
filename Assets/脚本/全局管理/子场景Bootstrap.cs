using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 子场景独立 Play 时的单例引导器。
//
// 项目的持久化单例（GameManager / 对话管理器 / 场景加载器 / InventoryManager / UIManager / 等）
// 都挂在 次元空间.scene 里，配 DontDestroyOnLoad。主流程从 序章 → 次元空间 进入后，
// 走任何子场景这些单例都存在。但开发者直接 Play 某个子场景（如王宫.scene）时，
// 这些单例还没创建，按 F 触发对话或加道具就会 NullRef。
//
// 本组件在 Awake 检测关键单例缺失，则 additive 加载"孵化场景"（默认次元空间）让单例 Awake 跑一次，
// 然后立刻 unload 该场景。DontDestroyOnLoad 标记的单例会保留下来，场景物件被自动清理。
//
// 主流程进入时单例已就绪，本组件什么也不做（零开销）。
public class 子场景Bootstrap : MonoBehaviour
{
    [Tooltip("缺失单例时 additive 加载哪个场景孵化单例。该场景必须挂全所有 DontDestroyOnLoad 单例。")]
    public string 单例孵化场景 = "次元空间";

    [Tooltip("孵化期间需要禁用可见性的根 GameObject 名（避免视觉/输入冲突）。")]
    public string[] 孵化时禁用的根名 = new string[]
    {
        "Main Camera", "背景", "主角", "平台", "传送门", "道具", "道具 (1)", "精灵", "演化UI", "之"
    };

    void Awake()
    {
        if (单例齐全()) return;
        StartCoroutine(引导());
    }

    bool 单例齐全()
    {
        return GameManager.Instance != null
            && 对话管理器.Instance != null
            && 场景加载器.Instance != null
            && InventoryManager.instance != null;
    }

    IEnumerator 引导()
    {
        Debug.LogWarning("[子场景Bootstrap] 检测到单例缺失，正在 additive load " + 单例孵化场景 + " 孵化单例…");
        var loadOp = SceneManager.LoadSceneAsync(单例孵化场景, LoadSceneMode.Additive);
        yield return loadOp;

        var scene = SceneManager.GetSceneByName(单例孵化场景);
        if (scene.IsValid() && scene.isLoaded)
        {
            // 禁用孵化场景里的视觉物件，避免与当前场景冲突
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var 名 in 孵化时禁用的根名)
                    if (root.name == 名) { root.SetActive(false); break; }
            }
        }

        yield return null; // 让 Awake/Start 跑完一轮

        var unloadOp = SceneManager.UnloadSceneAsync(单例孵化场景);
        yield return unloadOp;

        // 子场景独立 Play 时没有"开场自白"之类脚本调 UnlockInput，
        // PlayerInputLock.Start 已经把输入锁了，这里替它解一次。
        PlayerInputLock.inputLocked = false;

        Debug.Log("[子场景Bootstrap] 单例孵化完成: GM="
            + (GameManager.Instance != null) + " 对话="
            + (对话管理器.Instance != null) + " 加载器="
            + (场景加载器.Instance != null) + " 背包="
            + (InventoryManager.instance != null) + " 输入已解锁");
    }
}
