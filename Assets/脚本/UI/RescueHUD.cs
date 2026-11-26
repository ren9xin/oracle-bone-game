using TMPro;
using UnityEngine;

// 太学藏书阁 HUD：订阅 RescueTimer / RescueScore 事件刷新文本。
public class RescueHUD : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI 时间文本;
    public TextMeshProUGUI 得分文本;

    [Header("文案模板（{0}=救成功 {1}=错带 {2}=烧毁）")]
    public string 得分模板 = "救 {0}  错 {1}  烧 {2}";

    void Start()
    {
        if (RescueTimer.Instance != null)
        {
            RescueTimer.Instance.每秒tick += OnTimerTick;
            OnTimerTick(RescueTimer.Instance.剩余时间);
        }
        if (RescueScore.Instance != null)
        {
            RescueScore.Instance.状态变化 += OnScoreChanged;
            OnScoreChanged();
        }
    }

    void OnDestroy()
    {
        if (RescueTimer.Instance != null) RescueTimer.Instance.每秒tick -= OnTimerTick;
        if (RescueScore.Instance != null) RescueScore.Instance.状态变化 -= OnScoreChanged;
    }

    void OnTimerTick(float 剩余)
    {
        if (时间文本 != null) 时间文本.text = Mathf.CeilToInt(剩余).ToString();
    }

    void OnScoreChanged()
    {
        if (得分文本 == null || RescueScore.Instance == null) return;
        var s = RescueScore.Instance;
        得分文本.text = string.Format(得分模板, s.救成功数, s.错带数, s.烧毁数);
    }
}
