using UnityEngine;

// 长安诗会·飞花令的单题数据。每题一份 SO。
// 题面："下列哪句诗中有「{主题字}」字？" → 4 个候选诗句，正确索引 0~3。
[CreateAssetMenu(fileName = "新诗题", menuName = "唐朝/飞花令/诗题")]
public class PoetryQuestion : ScriptableObject
{
    [Header("题面")]
    public string 主题字;            // 例："花"

    [Tooltip("4 个候选诗句（原诗，含挖空字），长度应为 4。")]
    public string[] 候选诗句 = new string[4];

    [Tooltip("每个选项要挖掉的字（长度 4，对应 候选诗句）。\n规则：正确索引那项的 挖空字 == 主题字；其余 3 项是各自诗句中另一个标志字。\n显示时把该字替换为 〇。")]
    public string[] 选项挖空字 = new string[4];

    [Tooltip("正确答案的下标（0~3）。该项的 选项挖空字 应等于 主题字。")]
    public int 正确索引;

    [Header("元信息")]
    public string 出处;              // 例："李白《静夜思》"
    public string 注释;              // 可选：解释为什么这句里有主题字，给灵蝉吐槽用

    [Header("难度")]
    [Tooltip("1=简单，2=中等。学童关用 1，李白关用 2。")]
    [Range(1, 3)]
    public int 难度 = 1;
}
