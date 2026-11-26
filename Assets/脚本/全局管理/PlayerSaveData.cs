using UnityEngine;

[System.Serializable] // 必须加，才能被序列化
public class PlayerSaveData
{
    // 存档内容
    public string currentScene;  // 当前场景
    public int currentHealth;    // 血量
    public int moneycount;       // 金币
    public Inventory myInventory;// 背包
    public int maxHealth;        // 最大血量
    public int unlockedGateIndex; // 解锁到几号门

    // ====== 暗号道具演化系统（2026-05-15 新增） ======
    // 玩家是否已经从商王那里获得过 4 个甲骨碎片
    public bool 是否获得甲骨文;
    // 4 个甲骨字道具的字形演化阶段：0=甲骨, 1=秦篆, 2=唐楷
    public int 暗号字形阶段;

    // 灵蝉在次元空间的"首次到达"对白播放标记，防止反复进入次元空间重播
    public bool 灵蝉对白_商朝后已播;
    public bool 灵蝉对白_秦朝后已播;
    public bool 灵蝉对白_唐朝后已播;

    // 王宫穿越后血条UI/金币UI 进入永久隐藏状态
    public bool 血金UI隐藏;
}
