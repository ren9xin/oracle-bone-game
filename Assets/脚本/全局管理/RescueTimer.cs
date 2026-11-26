using System;
using UnityEngine;

// 太学藏书阁限时计时器。单例 + 自动开始。
// 每秒触发 每秒tick，时限到 触发 时限到（用于结算 / 触发烧毁等）。
public class RescueTimer : MonoBehaviour
{
    public static RescueTimer Instance;

    [Header("配置")]
    public float 总时长 = 60f;
    public bool 自动开始 = true;

    public float 剩余时间 { get; private set; }
    public bool 已结束 { get; private set; }
    public bool 进行中 { get; private set; }

    public event Action<float> 每秒tick;   // 余秒
    public event Action 时限到;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        剩余时间 = 总时长;
        if (自动开始) 开始();
    }

    public void 开始()
    {
        进行中 = true;
        已结束 = false;
        剩余时间 = 总时长;
        if (每秒tick != null) 每秒tick(剩余时间);
    }

    public void 暂停()
    {
        进行中 = false;
    }

    public void 继续()
    {
        if (!已结束) 进行中 = true;
    }

    // 罚时：扣减剩余时间。到 0 触发 时限到。
    public void 扣时间(float 秒)
    {
        if (已结束 || 秒 <= 0f) return;
        剩余时间 -= 秒;
        if (剩余时间 < 0f) 剩余时间 = 0f;
        if (每秒tick != null) 每秒tick(剩余时间);
        if (剩余时间 <= 0f)
        {
            进行中 = false;
            已结束 = true;
            if (时限到 != null) 时限到();
        }
    }

    void Update()
    {
        if (!进行中) return;
        float prev = 剩余时间;
        剩余时间 -= Time.deltaTime;
        if (剩余时间 < 0f) 剩余时间 = 0f;

        // 每秒边界触发一次
        if (Mathf.FloorToInt(prev) != Mathf.FloorToInt(剩余时间))
        {
            if (每秒tick != null) 每秒tick(剩余时间);
        }

        if (剩余时间 <= 0f && !已结束)
        {
            进行中 = false;
            已结束 = true;
            if (时限到 != null) 时限到();
        }
    }
}
