using TMPro;
using UnityEngine;

// 教学关（咸阳街市）专属：
// 1. UI 显示「甲骨字 X/Y」
// 2. 玩家收齐 requiredCount 个字之前，把 通关判断.PassAllow 锁 false（出口不放行）
// 3. 收齐后翻 true，文本切换提示
//
// 类名故意用 ASCII（不用中文）—— 通过 execute_code 的 CodeDOM 引用中文类名会 mojibake
public class TutorialGate : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI 进度文本;

    [Header("门槛")]
    public 通关判断 玩家通关判断;
    public int 必需数量 = 5;

    [Header("文案模板（{0}=已收集，{1}=必需）")]
    public string 模板_未达成 = "甲骨字  {0}/{1}";
    public string 模板_达成 = "甲骨字  {0}/{1}  · 可前往藏书阁";

    void Start()
    {
        if (字典管理器.Instance != null)
            字典管理器.Instance.字被收集 += OnZiCollected;
        Refresh();
    }

    void OnDestroy()
    {
        if (字典管理器.Instance != null)
            字典管理器.Instance.字被收集 -= OnZiCollected;
    }

    void OnZiCollected(甲骨字 _)
    {
        Refresh();
    }

    void Refresh()
    {
        int count = 字典管理器.Instance != null ? 字典管理器.Instance.已收集数量 : 0;
        bool reached = count >= 必需数量;

        if (进度文本 != null)
            进度文本.text = string.Format(reached ? 模板_达成 : 模板_未达成, count, 必需数量);

        if (玩家通关判断 != null)
            玩家通关判断.PassAllow = reached;
    }
}
