using System.Collections;
using UnityEngine;

// 次元空间 Hub：玩家每次回到次元空间时根据 GameManager.unlockedGateIndex 推进字形演化 + 触发灵蝉对白。
//
// unlockedGateIndex 与剧情节点的对应关系：
//   1：序章 → 商朝门可走（初始状态）
//   2：商朝（王宫）刚通关，玩家拿到 4 个甲骨碎片。秦朝门解锁。
//       → 首次到达可播"商朝后"灵蝉对白；字形保持 甲骨（不演化）
//   3：秦朝（太学藏书阁）刚通关。唐朝门解锁。
//       → 首次到达可播"秦朝后"灵蝉对白；演化 甲骨 → 秦篆
//   4：唐朝（长安诗会）刚通关。回家门解锁。
//       → 首次到达可播"唐朝后"灵蝉对白；演化 秦篆 → 唐楷
//
// 演化进度 + 对白播放标记 都存在 GameManager 上，跟随 PlayerSaveData 持久化。
// 主流程在 GameSaveManager.OnSceneLoaded 时已经把这些字段写回 GameManager。
public class 次元空间Hub : MonoBehaviour
{
    [Tooltip("演化反馈 UI（可选）。挂上后演化时会弹面板播淡入淡出；不挂则静默切换。")]
    public 字形演化动画 演化UI;

    [Tooltip("演化前的延迟（让场景过渡淡入完成）。")]
    public float 进场延迟秒 = 0.6f;

    [Tooltip("回家门 GameObject（gateIndex=4），唐朝通关后才显示。其他门一直可见，由 unlockedGateIndex 控制是否可入。")]
    public GameObject 回家门;

    [Header("灵蝉对白 SO（玩家自填）")]
    [Tooltip("首次完成商朝后回到次元空间触发。剧情：灵蝉对获取的 4 个甲骨碎片说点啥。")]
    public DialogueSO 灵蝉对白_商朝后;

    [Tooltip("首次完成秦朝后回到次元空间触发。剧情：碎片演化到秦篆，灵蝉解读。")]
    public DialogueSO 灵蝉对白_秦朝后;

    [Tooltip("首次完成唐朝后回到次元空间触发。剧情：碎片演化到唐楷，灵蝉提示走回家门。")]
    public DialogueSO 灵蝉对白_唐朝后;

    [Tooltip("灵蝉对白触发完成后等多久再启动演化动画（让对白先结束）。如果同时播会重叠。")]
    public float 对白后等演化秒 = 0.5f;

    public DialogueSO 待播对白 = null;// 决定本次到达的剧情节点 + 该播哪条对白 + 演化到哪个阶段
    void Awake()
    {
        // 提前一手解锁（Awake 在 PlayerInputLock.Start 之前；万一执行顺序倒置，Start 协程再兜底）
        PlayerInputLock.inputLocked = false;
    }

    void Start()
    {
        // 回家门控制：唐朝通关 且 唐朝后灵蝉对白已经播完 才显示
        // （首次唐朝穿越回来时这里是 false，等协程播完对白后再 SetActive(true)）
        if (回家门 != null) 回家门.SetActive(应显示回家门());

        // 玩家进入次元空间总是可移动：解锁全局输入锁 + 恢复主角 motion / isInDialogue
        StartCoroutine(解锁玩家());

        StartCoroutine(检查并演化());
    }

    // 等一帧让 PlayerInputLock.Start 先跑，再覆盖它的锁定
    IEnumerator 解锁玩家()
    {
        yield return null;
        PlayerInputLock.inputLocked = false;
        var p = FindObjectOfType<角色脚本>();
        if (p != null) { p.isInDialogue = false; p.MoveAxis(); }
    }

    bool 应显示回家门()
    {
        var gm = GameManager.Instance;
        return gm != null && gm.unlockedGateIndex >= 4 && gm.灵蝉对白_唐朝后已播;
    }

    IEnumerator 检查并演化()
    {
        yield return new WaitForSeconds(进场延迟秒);

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("次元空间Hub：GameManager 缺失，跳过演化");
            yield break;
        }

        var gm = GameManager.Instance;
        int idx = gm.unlockedGateIndex;

        
        int 目标演化 = gm.暗号字形阶段;   // 默认不变

        if (idx >= 4)// && !gm.灵蝉对白_唐朝后已播)
        {
            待播对白 = 灵蝉对白_唐朝后;
            gm.灵蝉对白_唐朝后已播 = true;
            目标演化 = 2;     // 唐楷
        }
        else if (idx >= 3)// && !gm.灵蝉对白_秦朝后已播)
        {
            待播对白 = 灵蝉对白_秦朝后;
            gm.灵蝉对白_秦朝后已播 = true;
            目标演化 = 1;     // 秦篆
        }
        else if (idx >= 2)// && !gm.灵蝉对白_商朝后已播)
        {
            待播对白 = 灵蝉对白_商朝后;
            gm.灵蝉对白_商朝后已播 = true;
            目标演化 = 0;     // 甲骨（不演化，但首次播对白）
        }
        else
        {
            // 重复进入次元空间（非首次）：单纯按 unlockedGateIndex 推到对应阶段（防进度落档）
            目标演化 = (idx >= 4) ? 2 : (idx >= 3) ? 1 : 0;
        }

        bool 需演化 = 目标演化 > gm.暗号字形阶段
                    && 暗号道具系统.Instance != null
                    && 暗号道具系统.Instance.道具已全部赋予();

        // 先播对白（若有）
        if (待播对白 != null && 对话管理器.Instance != null)
        {
            Debug.Log("次元空间Hub：播灵蝉对白 " + 待播对白.name);
            对话管理器.Instance.StartDialogue(待播对白);
            // 等对白结束
            while (对话管理器.Instance != null && 对话管理器.Instance.isInDialogue)
                yield return null;
            yield return new WaitForSeconds(对白后等演化秒);
        }

        // 再触发演化（若需要）
        if (需演化)
        {
            var 阶段 = (物品.字形阶段)目标演化;
            Debug.Log("次元空间Hub：演化 " + gm.暗号字形阶段 + " → " + 目标演化);
            if (演化UI != null)
                演化UI.播放演化(暗号道具系统.Instance.暗号道具集, 阶段);
            else
                暗号道具系统.Instance.演化到(阶段);
            gm.暗号字形阶段 = 目标演化;
        }

        // 对白 + 演化都完成后再判一次回家门（首次唐朝通关时 灵蝉对白_唐朝后已播 此时才 true）
        if (回家门 != null) 回家门.SetActive(应显示回家门());
    }
}
