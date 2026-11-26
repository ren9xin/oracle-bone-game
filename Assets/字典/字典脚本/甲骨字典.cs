using System.Collections.Generic;
using UnityEngine;

// 玩家已收集甲骨字的容器 SO。运行期由"字典管理器"读写。
// 在工程里通常只放一份实例（参考 Inventory 与 MyBag.asset 的关系）。
[CreateAssetMenu(fileName = "新字典", menuName = "字典/甲骨字典")]
public class 甲骨字典 : ScriptableObject
{
    public List<甲骨字> 已收集字 = new List<甲骨字>();
}
