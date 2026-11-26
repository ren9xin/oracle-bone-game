using UnityEngine;

// 配置编辑器右键菜单项，方便直接创建SO实例
[CreateAssetMenu(fileName = "NewNPC", menuName = "SO/NPC数据", order = 1)]
public class NPCSO : ScriptableObject
{
    [Header("NPC基础信息")]
    [Tooltip("NPC的显示名称")]
    public string npcName; // 角色名称

    [Tooltip("NPC的头像或立绘")]
    public Sprite npcSprite; // 角色图片
}
