using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 演化反馈 UI：进次元空间时短暂弹一个面板，4 个 icon 从原字形 fade out → swap → fade in。
// 自动隐藏。挂在 DontDestroyOnLoad 的 UI Canvas 上（由 次元空间Hub 创建/查找）。
public class 字形演化动画 : MonoBehaviour
{
    [Tooltip("根面板，演化时显示，结束后隐藏。")]
    public GameObject 根面板;

    [Tooltip("4 个 Image 槽位（按 之/马/开/门 顺序）。")]
    public Image[] icon槽 = new Image[4];

    [Tooltip("标题文本，例：「甲骨文 → 小篆」。")]
    public TextMeshProUGUI 标题文本;

    [Header("时序")]
    public float 淡出秒 = 0.5f;
    public float 停留秒 = 0.8f;
    public float 淡入秒 = 0.5f;

    void Awake()
    {
        if (根面板 != null) 根面板.SetActive(false);
    }

    public void 播放演化(物品[] 道具集, 物品.字形阶段 目标阶段)
    {
        if (道具集 == null || 道具集.Length == 0) return;
        if (根面板 == null)
        {
            // 没接面板就退化为静默切换
            foreach (var 道具 in 道具集)
                if (道具 != null) 道具.应用字形阶段(目标阶段);
            return;
        }
        StartCoroutine(协程(道具集, 目标阶段));
    }

    IEnumerator 协程(物品[] 道具集, 物品.字形阶段 目标阶段)
    {
        根面板.SetActive(true);

        // 标题
        if (标题文本 != null)
        {
            标题文本.text = 目标阶段 == 物品.字形阶段.秦篆 ? "甲骨文 → 秦小篆"
                          : 目标阶段 == 物品.字形阶段.唐楷 ? "秦小篆 → 唐楷书"
                          : "字形变换";
        }

        // 1) 先展示当前字形 + 1 帧
        for (int i = 0; i < icon槽.Length; i++)
        {
            if (icon槽[i] == null) continue;
            var 道具 = i < 道具集.Length ? 道具集[i] : null;
            if (道具 != null && 道具.itemImage != null)
            {
                icon槽[i].sprite = 道具.itemImage;
                icon槽[i].color = new Color(1, 1, 1, 1);
                icon槽[i].enabled = true;
            }
            else
            {
                icon槽[i].enabled = false;
            }
        }
        yield return null;

        // 2) 淡出
        yield return 淡(1f, 0f, 淡出秒);

        // 3) 切换 sprite（更新 SO + UI）
        for (int i = 0; i < icon槽.Length; i++)
        {
            var 道具 = i < 道具集.Length ? 道具集[i] : null;
            if (道具 == null) continue;
            道具.应用字形阶段(目标阶段);
            if (icon槽[i] != null && 道具.itemImage != null)
                icon槽[i].sprite = 道具.itemImage;
        }

        // 4) 停留
        yield return new WaitForSeconds(Mathf.Max(0.05f, 停留秒));

        // 5) 淡入实际上是淡回到不透明（停留时 alpha=0；这里反向）
        yield return 淡(0f, 1f, 淡入秒);

        // 6) 再停留一拍，给玩家看清新字形
        yield return new WaitForSeconds(0.6f);

        根面板.SetActive(false);

        // 演化结束顺手刷新背包 UI（玩家如果开着背包能看到 icon 立刻变）
        InventoryManager.强制刷新UI();
    }

    IEnumerator 淡(float 起始alpha, float 目标alpha, float 时长)
    {
        if (时长 <= 0f)
        {
            for (int i = 0; i < icon槽.Length; i++)
                if (icon槽[i] != null) icon槽[i].color = new Color(1, 1, 1, 目标alpha);
            yield break;
        }
        float t = 0f;
        while (t < 时长)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(起始alpha, 目标alpha, t / 时长);
            for (int i = 0; i < icon槽.Length; i++)
                if (icon槽[i] != null) icon槽[i].color = new Color(1, 1, 1, a);
            yield return null;
        }
    }
}
