using UnityEngine;

// 太学藏书阁时限耗尽 → 弹出"游戏结束"面板。
// 面板里的"重新开始"按钮挂 UIButton(BtnType=重新开始) 即可重载本场景。
public class RescueGameOver : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject 结束面板;     // 默认在 Inspector 里设置为隐藏

    [Header("行为")]
    public bool 暂停游戏 = true;    // 弹板时冻结时间

    void Start()
    {
        // 进入场景先把 timeScale 恢复（防上一局留下的 0）
        Time.timeScale = 1f;
        if (结束面板 != null) 结束面板.SetActive(false);
        if (RescueTimer.Instance != null) RescueTimer.Instance.时限到 += 显示结束;
    }

    void OnDestroy()
    {
        if (RescueTimer.Instance != null) RescueTimer.Instance.时限到 -= 显示结束;
    }

    void 显示结束()
    {
        if (结束面板 != null) 结束面板.SetActive(true);
        if (暂停游戏) Time.timeScale = 0f;
    }
}
