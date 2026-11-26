using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance;

    public Inventory myInventory;
    private PlayerSaveData _loadedData;

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

        // 监听场景加载完成
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ======================
    // 存档（自动找玩家）
    // ======================
    public void SaveGame()
    {
        角色脚本 player = FindPlayer();
        if (player == null)
        {
            Debug.LogWarning("找不到玩家，无法存档");
            return;
        }

        string path = Application.persistentDataPath + "/game_SaveData";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        PlayerSaveData data = new PlayerSaveData();
        data.currentScene = SceneManager.GetActiveScene().name;
        data.currentHealth = GameManager.Instance.currentHealth;
        data.maxHealth = GameManager.Instance.maxHealth;
        data.moneycount = GameManager.Instance.moneyCount;
        data.unlockedGateIndex = GameManager.Instance.unlockedGateIndex;
        data.myInventory = myInventory;

        // 暗号道具演化 + 灵蝉对白进度
        data.是否获得甲骨文 = GameManager.Instance.是否获得甲骨文;
        data.暗号字形阶段 = GameManager.Instance.暗号字形阶段;
        data.灵蝉对白_商朝后已播 = GameManager.Instance.灵蝉对白_商朝后已播;
        data.灵蝉对白_秦朝后已播 = GameManager.Instance.灵蝉对白_秦朝后已播;
        data.灵蝉对白_唐朝后已播 = GameManager.Instance.灵蝉对白_唐朝后已播;
        data.血金UI隐藏 = GameManager.Instance.血金UI隐藏;

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream file = File.Create(path + "/saveData.txt"))
        {
            string json = JsonUtility.ToJson(data);
            formatter.Serialize(file, json);
        }

        Debug.Log("存档成功");
    }

    // ======================
    // 读档（只加载数据）
    // ======================
    public void LoadGame()
    {
        StartCoroutine(LoadGameAfterCloseUI());

    }

    private IEnumerator LoadGameAfterCloseUI()
    {
        // 先强制关掉所有UI
        if (UIManager.Instance != null)
            UIManager.Instance.CloseAllUI();

        // 再找一遍，强行隐藏
        var settings = GameObject.Find("Settings");
        if (settings != null) settings.SetActive(false);

        var bag = GameObject.Find("myBag");
        if (bag != null) bag.SetActive(false);

        // 等一帧，让Unity真正执行关闭
        yield return null;

        string path = Application.persistentDataPath + "/game_SaveData/saveData.txt";
        if (!File.Exists(path))
        {
            Debug.LogWarning("无存档文件");
            yield break;
        }

        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Open(path, FileMode.Open))
        {
            string json = (string)bf.Deserialize(file);
            _loadedData = JsonUtility.FromJson<PlayerSaveData>(json);
        }

        // 切换场景
        if (!string.IsNullOrEmpty(_loadedData.currentScene))
        {
            SceneManager.LoadScene(_loadedData.currentScene);
        }
    }

    // ======================
    // 场景加载完毕 → 自动给玩家赋值
    // ======================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 每次场景加载完，按当前标志刷新血条UI/金币UI 显隐（处理 DontDestroyOnLoad UI 跨场景延续）
        if (GameManager.Instance != null) GameManager.Instance.刷新血金UI();

        // ========== 新增：死亡重生初始化 ==========
        角色脚本 player = FindPlayer();
        if (player != null)
        {
            // 从GameManager刷新满血、零金币
            player.currentHealth = GameManager.Instance.currentHealth;
            player.moneycount = GameManager.Instance.moneyCount;

            // 重置玩家状态：解除死亡、恢复移动、重置动画
            player.motion = true;
            player.isInDialogue = false;
            if (player.ani != null)
            {
                player.ani.ResetTrigger("IsDead");
            }

            // 刷新血条、金币UI
            if (player.healthBar != null)
                player.healthBar.RefreshFromGM();
            if (player.money != null)
                player.money.RefreshFromGM();
        }
        if (_loadedData == null) return;

        //角色脚本 player = FindPlayer();
        if (player != null)
        {
            // 先把读档数据写入全局 GameManager
            GameManager.Instance.SetPlayerData(
                _loadedData.currentHealth,
                _loadedData.moneycount,
                _loadedData.maxHealth
            );
            GameManager.Instance.SetUnlockedGate(_loadedData.unlockedGateIndex);

            // 暗号道具演化 + 灵蝉对白进度（同步回 GameManager 字段；演化阶段 SO 字形由 暗号道具系统.Start 读 GameManager 后应用）
            GameManager.Instance.暗号字形阶段 = _loadedData.暗号字形阶段;
            GameManager.Instance.灵蝉对白_商朝后已播 = _loadedData.灵蝉对白_商朝后已播;
            GameManager.Instance.灵蝉对白_秦朝后已播 = _loadedData.灵蝉对白_秦朝后已播;
            GameManager.Instance.灵蝉对白_唐朝后已播 = _loadedData.灵蝉对白_唐朝后已播;
            GameManager.Instance.血金UI隐藏 = _loadedData.血金UI隐藏;
            GameManager.Instance.是否获得甲骨文 = _loadedData.是否获得甲骨文;
            GameManager.Instance.暗号字形阶段 = _loadedData.暗号字形阶段;
            // 处理背包里的 4 个甲骨碎片：
            //   未获得 → 清空背包（防存档里的旧 itemList 残留）
            //   已获得 → 把 4 个甲骨字 SO 演化到存档里的阶段；其他物品保持原样
            if (!_loadedData.是否获得甲骨文)
            {
                if (myInventory != null && myInventory.itemList != null) myInventory.itemList.Clear();
            }
            else if (暗号道具系统.Instance != null)
            {
                暗号道具系统.Instance.演化到((物品.字形阶段)_loadedData.暗号字形阶段);
            }

            // 赋值给角色本地
            player.currentHealth = _loadedData.currentHealth;
            player.moneycount = _loadedData.moneycount;

            // 刷新UI（从全局数据刷新）
            if (player.healthBar != null)
                player.healthBar.RefreshFromGM();
            if (player.money != null)
                player.money.RefreshFromGM();

            // 背包刷新
            myInventory = _loadedData.myInventory;
            if (InventoryManager.instance != null)
            {
                InventoryManager.RefreshItem();
            }

            Debug.Log("读档数据已自动应用到玩家");
        }

        // 应用完就清空，避免重复覆盖
        _loadedData = null;
    }

    // ======================
    // 全局找玩家（自动查找）
    // ======================
    private 角色脚本 FindPlayer()
    {
        return FindObjectOfType<角色脚本>();
    }
}
