using UnityEngine;

/// <summary>
/// NPC对话触发脚本：基于Collider Trigger实现范围检测与交互提示
/// </summary>
public class NPC对话 : MonoBehaviour
{
    [Header("对话配置")]
    [Tooltip("触发的起始对话节点")]
    public DialogueSO startDialogue; // 关联之前创建的对话数据SO
    public DialogueSO normalDialogue;//常态对话（触发过第一次对话后保持不变的对话）

    [Header("交互提示配置")]
    [Tooltip("NPC头顶的感叹号提示UI（需设置为世界空间UI或NPC子物体）")]
    public GameObject exclamationMark; // 感叹号提示对象
    [Tooltip("感叹号显示偏移量（相对于NPC位置）")]
    public Vector3 exclamationOffset = new Vector3(0, 2.5f, 0); // 默认头顶2.5单位

    [Header("状态控制")]
    private bool isPlayerInRange = false; // 玩家是否在交互范围内
    private bool isInDialogue = false;     // 是否正在对话中
    private bool isNew = true;//是否是新对话

    void Start()
    {
        // 初始隐藏感叹号提示
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
            // 设置感叹号位置为NPC头顶
            UpdateExclamationPosition();
        }

        // 验证对话数据是否赋值
        if (startDialogue == null)
        {
            Debug.LogError($"[{gameObject.name}] 未配置起始对话节点，请在Inspector中绑定DialogueSO！");
        }
    }

    void Update()
    {
        // 仅在玩家在范围内且未对话时处理F输入
        if (isPlayerInRange && !isInDialogue && startDialogue != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                // 触发对话
                StartNPCDialogue();
            }
        }

        // 持续更新感叹号位置（若NPC移动）
        if (exclamationMark != null && exclamationMark.activeSelf)
        {
            UpdateExclamationPosition();
        }
    }

    #region 触发器事件处理
    /// <summary>
    /// 玩家进入触发范围
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // 显示感叹号提示
            if (exclamationMark != null)
            {
                exclamationMark.SetActive(true);
            }
            Debug.Log($"[{gameObject.name}] 玩家进入对话范围，按空格开始对话！");
        }
    }

    /// <summary>
    /// 玩家离开触发范围
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // 隐藏感叹号提示
            if (exclamationMark != null)
            {
                exclamationMark.SetActive(false);
            }

            // 若正在对话则终止
            if (isInDialogue && 对话管理器.Instance != null)
            {
                对话管理器.Instance.EndDialogue();
                isInDialogue = false;
            }
            Debug.Log($"[{gameObject.name}] 玩家离开对话范围！");
        }
    }
    #endregion

    #region 对话控制方法
    /// <summary>
    /// 开始NPC对话
    /// </summary>
    private void StartNPCDialogue()
    {
        if (对话管理器.Instance == null)
        {
            Debug.LogError("场景中未找到DialogueManager单例对象！");
            return;
        }

        isInDialogue = true;
        // 隐藏感叹号提示
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
        // 调用对话管理器开始对话
        if (isNew)//新对话
        {
            对话管理器.Instance.StartDialogue(startDialogue);
            isNew = false;
        }
        else
        {
            对话管理器.Instance.StartDialogue(normalDialogue);
        }
        
    }
    #endregion

    #region UI位置更新
    /// <summary>
    /// 更新感叹号位置到NPC头顶
    /// </summary>
    private void UpdateExclamationPosition()
    {
        exclamationMark.transform.position = transform.position + exclamationOffset;
    }
    #endregion
}
