using UnityEngine;

public class SceneInit : MonoBehaviour
{
    [Header("幕布配置")]
    public Animator curtainAnimator; // 拖入幕布的Animator组件
    public string curtainTriggerName = "Image"; // 动画触发参数名（和控制器一致）
    public float delayTime = 0f; // 延迟触发时间（0=场景加载后立刻触发）

    // 场景加载完成后立刻执行（早于Update）
    void Start()
    {
        // 安全检查：Animator未配置时打印警告
        if (curtainAnimator == null)
        {
            Debug.LogWarning("未配置幕布Animator组件！");
            return;
        }

        // 延迟触发（可选，比如想等0.5秒再拉开幕布）
        Invoke("PlayCurtainAnimation", delayTime);
    }

    // 播放幕布动画
    void PlayCurtainAnimation()
    {
        // 方式1：用Trigger触发动画（推荐，一次性播放）
        curtainAnimator.SetTrigger(curtainTriggerName);

        // 方式2：用Bool参数（如果是循环/持续状态动画，比如幕布保持打开）
        // curtainAnimator.SetBool("IsCurtainOpen", true);
    }
}
