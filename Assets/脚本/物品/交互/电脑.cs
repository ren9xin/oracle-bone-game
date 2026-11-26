using System.Collections;
using UnityEngine;

public class 电脑 : MonoBehaviour
{
    Animator ani;

    [Header("对话配置")]
    [Tooltip("延迟3秒后要触发的对话数据SO")]
    public DialogueSO dialogueAfterError; // 拖入你的对话数据SO
    [Tooltip("对话管理器（拖入场景中的DialogueManager物体）")]
    public 对话管理器 dialogueManager; // 绑定对话管理器

    void Start()
    {
        ani = GetComponent<Animator>();

        // 安全检查：自动查找对话管理器（避免手动拖拽）
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<对话管理器>();
        }
    }

    // 触发故障动画 + 3秒后触发对话
    public void error()
    {
        // 1. 播放故障动画
        ani.SetTrigger("error");

        // 2. 启动协程，延迟3秒触发对话
        StartCoroutine(TriggerDialogueAfterDelay(3f));
    }

    /// <summary>
    /// 延迟指定秒数后触发对话
    /// </summary>
    /// <param name="delayTime">延迟时间（秒）</param>
    IEnumerator TriggerDialogueAfterDelay(float delayTime)
    {
        // 等待指定秒数（动画播放3秒）
        yield return new WaitForSeconds(delayTime);

        // 安全检查：对话管理器和对话数据不为空才执行
        if (dialogueManager != null && dialogueAfterError != null)
        {
            dialogueManager.StartDialogue(dialogueAfterError);
            Debug.Log("电脑动画播放3秒后，触发对话：" + dialogueAfterError.name);
        }
        else
        {
            Debug.LogWarning("请检查对话管理器或对话数据SO是否配置！");
        }
    }
}
