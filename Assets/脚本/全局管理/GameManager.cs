using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("关卡进度")]
    public int unlockedGateIndex = 1;// 当前解锁到第几号门

    [Header("全局角色数据")]
    public int currentHealth;
    public int maxHealth;
    public int moneyCount;

    [Header("暗号道具演化系统（跨章节存档）")]
    [Tooltip("玩家是否已经在王宫从商王那里获得过 4 个甲骨碎片。false 时读档清空背包。")]
    public bool 是否获得甲骨文 = false;

    [Tooltip("4 个甲骨字道具的字形阶段：0=甲骨, 1=秦篆, 2=唐楷。由 次元空间Hub 推进。")]
    public int 暗号字形阶段 = 0;

    [Header("灵蝉对白播放标记（防止反复进次元空间重播）")]
    public bool 灵蝉对白_商朝后已播;
    public bool 灵蝉对白_秦朝后已播;
    public bool 灵蝉对白_唐朝后已播;

    [Tooltip("血条UI / 金币UI 是否进入「穿越后隐藏」状态。王宫→次元空间时设 true，跨场景永久。")]
    public bool 血金UI隐藏;


    //  核心事件：门解锁时广播
    public event Action OnGateUnlocked;
    // 事件：只要数据变了就触发
    public event Action OnDataChanged;

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

    // 外部调用：解锁下一个门
    public void UnlockNextGate()
    {
        unlockedGateIndex++;
        // 广播事件：所有门都会收到
        OnGateUnlocked?.Invoke();
    }

    // 直接设置解锁到几号门（用于读档/初始化）
    public void SetUnlockedGate(int index)
    {
        unlockedGateIndex = index;
        OnGateUnlocked?.Invoke();
    }

    // 一次性设置所有角色数据（读档专用）
    public void SetPlayerData(int health, int money, int maxHp)
    {
        maxHealth = maxHp;
        currentHealth = health;
        moneyCount = money;
        // 数据变了！通知所有UI刷新
        OnDataChanged?.Invoke();
    }

    // 按 血金UI隐藏 标志 SetActive 血条UI/金币UI GameObject。
    // 在 暗号道具系统.传送回次元空间() 设标志 + 在 GameSaveManager.OnSceneLoaded 每次场景加载完毕调一次。
    public void 刷新血金UI()
    {
        var hp = GameObject.Find("血条UI");
        var coin = GameObject.Find("金钱UI");   // 场景里实际命名是「金钱UI」不是「金币UI」
        if (hp != null) hp.SetActive(!血金UI隐藏);
        if (coin != null) coin.SetActive(!血金UI隐藏);
    }

    // ======================
    // 死亡后重新开始：当前场景初始化
    // ======================
    public void PlayerRestartCurrentScene()
    {
        Debug.Log("正在重新生成场景");
        // 1. 获取当前场景名，重新加载当前场景
        string currentScene = SceneManager.GetActiveScene().name;

        // 2. 重置游戏全局数据：血量回满、金币归零、门解锁进度保留（可按需改）
        角色脚本 tempPlayer = FindObjectOfType<角色脚本>();
        int maxHp = tempPlayer != null ? tempPlayer.defaultMaxHealth : 100;

        // 血量回满最大值、金币清零
        GameManager.Instance.SetPlayerData(maxHp, 0, maxHp);
        // 门解锁保持不变，不用改 GameManager.Instance.unlockedGateIndex
        GameObject deadUI = GameObject.Find("死亡界面");
        if(deadUI != null)
        {
            deadUI.SetActive(false);
        }
        // 3. 重新加载当前场景
        SceneManager.LoadScene(currentScene);
    }
}