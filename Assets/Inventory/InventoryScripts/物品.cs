using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Item",menuName ="Inventory/New Item")]
public class 物品 : ScriptableObject
{
    public enum 字形阶段 { 甲骨 = 0, 秦篆 = 1, 唐楷 = 2 }

    public string itemName;
    public Sprite itemImage;
    public int itemHeld;//持有数量
    [TextArea]
    public string itemInfo;//物品描述
    public bool equip;

    [Header("跨章节字形演化（暗号道具用，其他物品留空即可）")]
    public Sprite 图_甲骨;
    public Sprite 图_秦篆;
    public Sprite 图_唐楷;

    [Header("跨章节字形演化的描述文本（与 图_* 配对，演化时一并替换）")]
    [TextArea] public string info_甲骨;
    [TextArea] public string info_秦篆;
    [TextArea] public string info_唐楷;

    [Tooltip("当前显示的字形阶段。仅暗号道具有效，运行时由 暗号道具系统 修改。")]
    public 字形阶段 当前字形阶段 = 字形阶段.甲骨;

    // 把 itemImage / itemInfo 切到指定阶段对应的 Sprite + 文本。
    // 非暗号道具（三阶段图全空）调用此方法不会被修改。
    public void 应用字形阶段(字形阶段 阶段)
    {
        Sprite 新图 = null;
        string 新文 = null;
        switch (阶段)
        {
            case 字形阶段.甲骨: 新图 = 图_甲骨; 新文 = info_甲骨; break;
            case 字形阶段.秦篆: 新图 = 图_秦篆; 新文 = info_秦篆; break;
            case 字形阶段.唐楷: 新图 = 图_唐楷; 新文 = info_唐楷; break;
        }
        bool 已变 = false;
        if (新图 != null) { itemImage = 新图; 已变 = true; }
        if (!string.IsNullOrEmpty(新文)) { itemInfo = 新文; 已变 = true; }
        if (已变) 当前字形阶段 = 阶段;
    }
}
