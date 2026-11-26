using UnityEngine;

// 单个甲骨字的数据定义。每个甲骨字一份 SO 资源。
[CreateAssetMenu(fileName = "新甲骨字", menuName = "字典/甲骨字")]
public class 甲骨字 : ScriptableObject
{
    [Header("字形")]
    public string 现代字;          // 例："日"
    public Sprite 字形图;          // 甲骨文字形 sprite，透明 PNG

    [TextArea]
    public string 释义;            // 例："太阳。象形字，圆形或椭圆中加一点。"
}
