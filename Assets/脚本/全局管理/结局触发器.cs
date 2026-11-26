using UnityEngine;
using System.Collections;

// 结局场景的极简编排：场景加载后短暂延迟，触发"精灵告别"对白。
// 对白末尾 callback 调本组件的 结束游戏() → 回到 开始界面。
public class 结局触发器 : MonoBehaviour
{
    [Tooltip("精灵告别对白的根节点。链路末尾 callback 必须调用 结局系统.结束游戏。")]
    public DialogueSO 告别对白;

    [Tooltip("回到 Build Settings 中的哪个场景。默认为 开始界面。")]
    public string 结束后场景 = "开始界面";

    [Tooltip("场景加载后延迟多少秒触发对白。")]
    public float 触发延迟秒 = 0.6f;

    public GameObject 结局画面;

    void Start()
    {
        Invoke("触发告别", Mathf.Max(0f, 触发延迟秒));
    }

    void 触发告别()
    {
        if (告别对白 == null || 对话管理器.Instance == null)
        {
            Debug.LogWarning("结局触发器: 告别对白 或 对话管理器.Instance 缺失，跳过对白直接结束游戏。");
            结束游戏();
            return;
        }
        对话管理器.Instance.StartDialogue(告别对白);
      
    }

    // 由 DialogueSO.callbacks 通过 SendMessage 调用（无参方法）
    // 如果 结束后场景 非空 → 跳到该场景；否则 → 真正退出游戏（结局自白末尾用这条分支）
    public void 结束游戏()
    {
        if (!string.IsNullOrEmpty(结束后场景))
        {
            if (场景加载器.Instance != null)
                场景加载器.Instance.TransitionToScene(结束后场景);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(结束后场景);
            return;
        }
        // 没指定场景 = 退出游戏
        Debug.Log("结局触发器：游戏完整结束");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 兼容 / 简化命名：对白末尾直接 SendMessage 退出游戏（不通过 结束后场景）
    public void 退出游戏()
    {
        // 先打开结局画面
        结局画面.SetActive(true);
        Debug.Log("结局触发器：显示结局画面，5秒后退出游戏");

        // 开启协程等待5秒再退出
        //StartCoroutine(ExitAfterDelay(5f));
    }

    // 等待延迟后退出游戏
    IEnumerator ExitAfterDelay(float delay)
    {
        // 等待指定秒数
        yield return new WaitForSeconds(delay);

        Debug.Log("结局触发器：游戏完整结束（退出游戏）");

        // 退出游戏
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
