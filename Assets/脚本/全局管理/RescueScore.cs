using System;
using UnityEngine;

// 太学藏书阁抢救竹简的得分中心。单例。
// 三种事件源：玩家拾取真拓本（救成功）、玩家拾取伪竹简（错带）、火势烧毁竹简（烧毁）。
// 错带不扣命，改为扣倒计时秒数；时间归零由 RescueTimer 触发"时限到" → 游戏结束。
public class RescueScore : MonoBehaviour
{
    public static RescueScore Instance;

    [Header("当前状态")]
    public int 救成功数;
    public int 错带数;
    public int 烧毁数;

    //public int 玩家生命 = 3;

    //public int 错带扣命 = 1;


    [Header("规则")]
    public float 错带扣秒 = 10f;


    // 通知 UI 刷新
    public event Action 状态变化;
    public event Action<甲骨字> 救成功;       // 真拓本被救起，UI 可展示一下

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void 记一次救成功(甲骨字 关联字)
    {
        救成功数++;
        // 顺手把关联甲骨字加进字典（玩家在藏书阁里也能学新字）
        if (关联字 != null && 字典管理器.Instance != null)
            字典管理器.Instance.收集字(关联字);
        if (救成功 != null) 救成功(关联字);
        if (状态变化 != null) 状态变化();
    }

    public void 记一次错带()
    {
        错带数++;
        //玩家生命 -= 错带扣命;
        if (状态变化 != null) 状态变化();
        //if (玩家生命 <= 0 && 生命耗尽 != null) 生命耗尽();

        if (RescueTimer.Instance != null) RescueTimer.Instance.扣时间(错带扣秒);
        if (状态变化 != null) 状态变化();

    }

    public void 记一次烧毁()
    {
        烧毁数++;
        if (状态变化 != null) 状态变化();
    }

    public void 重置()
    {
        救成功数 = 0;
        错带数 = 0;
        烧毁数 = 0;
        if (状态变化 != null) 状态变化();
    }
}
