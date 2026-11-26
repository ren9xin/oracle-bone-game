using UnityEngine;

// 秦朝太学藏书阁的通关跳转：
//   - 救成功数 ≥ 通关救数目标 → 立刻判定通关
//   - 时限到 + 救成功数 ≥ 1 → 部分救援也算通关（玩家拼命救一根就够）
//   - 时限到 + 救成功数 = 0 → 走 RescueGameOver 的失败面板（不在本组件范围）
// 通关后：UnlockNextGate（让次元空间的唐朝门开） + 跳转到次元空间。
// 挂在场景中一个常驻 GameObject（建议挂在 RescueScore 同一个 GameObject 上）。
public class 秦朝通关 : MonoBehaviour
{
    [Tooltip("救满几根真竹简算通关。")]
    public int 通关救数目标 = 1;

    [Tooltip("通关后传送到的场景名。")]
    public string 下一场景 = "次元空间";

    [Tooltip("判定通关后等多久再跳转（给玩家看到「救完了」反馈的时间）。")]
    public float 跳转延迟秒 = 1.5f;

    bool 已通关 = false;

    void Start()
    {
        if (RescueScore.Instance != null) RescueScore.Instance.救成功 += On救成功;
        if (RescueTimer.Instance != null) RescueTimer.Instance.时限到 += On时限到;
    }

    void OnDestroy()
    {
        if (RescueScore.Instance != null) RescueScore.Instance.救成功 -= On救成功;
        if (RescueTimer.Instance != null) RescueTimer.Instance.时限到 -= On时限到;
    }

    void On救成功(甲骨字 字)
    {
        if (已通关) return;
        if (RescueScore.Instance != null && RescueScore.Instance.救成功数 >= 通关救数目标)
        {
            通关();
        }
    }

    void On时限到()
    {
        if (已通关) return;
        // 时限到 + 救到至少一根 → 部分救援算通关
        // 时限到 + 0 救援 → 失败，交给 RescueGameOver 弹板
        if (RescueScore.Instance != null && RescueScore.Instance.救成功数 >= 1)
        {
            通关();
        }
    }

    void 通关()
    {
        已通关 = true;
        if (RescueTimer.Instance != null) RescueTimer.Instance.暂停();
        if (GameManager.Instance != null) GameManager.Instance.UnlockNextGate();
        Debug.Log($"✅ 秦朝通关：救数 {(RescueScore.Instance != null ? RescueScore.Instance.救成功数 : -1)} → 跳转 {下一场景}");
        Invoke(nameof(跳转), 跳转延迟秒);
    }

    void 跳转()
    {
        Time.timeScale = 1f;  // 防止上一帧被暂停
        if (场景加载器.Instance != null)
            场景加载器.Instance.TransitionToScene(下一场景);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(下一场景);
    }
}
