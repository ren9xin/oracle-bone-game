using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class 对话管理器 : MonoBehaviour
{
    // 单例实例（确保全局唯一）
    public static 对话管理器 Instance { get; private set; }

    // 原有UI/配置字段...
    [Header("NPC信息UI")]
    public Image npcPortrait;
    public GameObject NPCNameRoot;
    public TextMeshProUGUI npcNameText;

    [Header("对话内容UI")]
    public GameObject dialogueContentRoot;
    public TextMeshProUGUI dialogueContentText;
    public Transform optionsGroup;
    public GameObject optionButtonPrefab;

    [Header("对话设置")]
    public bool useTypingEffect = true;
    public float typingSpeed = 0.05f;

    public bool isInDialogue { get; private set; }
    private DialogueSO currentDialogueNode;
    private int currentDialogueIndex = 0;
    private bool isTyping;
    private Coroutine typingCoroutine;
    角色脚本 player;

    // ========== 核心修改：单例+跨场景不销毁 ==========
    void Awake()
    {
        // 1. 如果已有单例（其他场景的），销毁当前物体
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 2. 设置为全局唯一单例，且跨场景不销毁
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 3. UI跟随场景切换（可选：如果UI在Canvas中，确保Canvas不销毁）
        // 若对话UI是子物体，无需额外操作；若UI在其他物体，需同步设置DontDestroyOnLoad
        if (dialogueContentRoot != null)
        {
            DontDestroyOnLoad(dialogueContentRoot.transform.root.gameObject); // Canvas不销毁
        }
        HideDialogueUI();
    }

    // 原有方法（Update、StartDialogue、InvokeCurrentNodeCallback等）保持不变...
    void Update()
    {
        if (isInDialogue && currentDialogueNode != null)
        {
            bool canInput = currentDialogueNode.dialogueOptions.Length == 0 || currentDialogueIndex < currentDialogueNode.dialogueContent.Length;

            if (canInput)
            {
                if (isTyping)
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                    {
                        StopTypingCoroutine();
                        dialogueContentText.text = currentDialogueNode.dialogueContent[currentDialogueIndex];
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                    {
                        NextDialogueLine();
                    }
                }
            }
        }
    }

    public void StartDialogue(DialogueSO startDialogue)
    {
        if (startDialogue == null) return;
        // 找到你的主角
        player = FindObjectOfType<角色脚本>();
        if (player != null)
        {
            player.isInDialogue = true;
            player.MoveBaned(); // 禁用移动
        } 
        
        isInDialogue = true;
        currentDialogueNode = startDialogue;
        currentDialogueIndex = 0;

        ShowDialogueUI();
        LoadDialogueNodeData();
    }

    private void LoadDialogueNodeData()
    {
        if (currentDialogueNode == null) return;

        if (currentDialogueNode.npcData != null)
        {
            npcPortrait.sprite = currentDialogueNode.npcData.npcSprite;
            npcNameText.text = currentDialogueNode.npcData.npcName;
        }

        ClearOptions();
        HideOptions();

        if (currentDialogueIndex < currentDialogueNode.dialogueContent.Length)
        {
            ShowCurrentDialogueText();
        }
        else
        {
            HandleDialogueEndOrBranch();
        }
    }

    private void ShowCurrentDialogueText()
    {
        if (useTypingEffect)
        {
            typingCoroutine = StartCoroutine(TypeDialogueText(currentDialogueNode.dialogueContent[currentDialogueIndex]));
        }
        else
        {
            dialogueContentText.text = currentDialogueNode.dialogueContent[currentDialogueIndex];
        }
    }

    private void HandleDialogueEndOrBranch()
    {
        InvokeCurrentNodeCallback();
        if (currentDialogueNode == null)
        {
            EndDialogue();
            return;
        }
        if (currentDialogueNode.dialogueOptions.Length > 0)
        {
            ShowOptions(currentDialogueNode.dialogueOptions);
        }
        else if (currentDialogueNode.NextDialogueNode != null)
        {
            currentDialogueNode = currentDialogueNode.NextDialogueNode;
            currentDialogueIndex = 0;
            LoadDialogueNodeData();
        }
        else
        {
            EndDialogue();
        }
    }

    private void NextDialogueLine()
    {
        currentDialogueIndex++;
        LoadDialogueNodeData();
    }

    public void EndDialogue()
    {
        if (player != null)
        {
            player.isInDialogue = false;
            player.MoveAxis();  // 恢复移动
        }
        isInDialogue = false;
        currentDialogueNode = null;
        currentDialogueIndex = 0;
        StopTypingCoroutine();
        HideDialogueUI();
    }

    private void InvokeCurrentNodeCallback()
    {
        if (currentDialogueNode == null) return;
        if (currentDialogueNode.callbacks == null) return;

        //  遍历所有回调，全部执行
        foreach (var callback in currentDialogueNode.callbacks)
        {
            string objName = callback.targetObjectName;
            string funcName = callback.callbackFunctionName;

            if (string.IsNullOrEmpty(objName) || string.IsNullOrEmpty(funcName))
                continue;

            GameObject targetObj = GameObject.Find(objName);
            if (targetObj == null)
            {
                Debug.LogWarning($"找不到物体：{objName}");
                continue;
            }

            // 调用函数
            targetObj.SendMessage(funcName, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"✅ 回调执行：{objName}.{funcName}()");
        }
    }

    private void ShowOptions(DialogueSO.DialogueOption[] options)
    {
        // 1. 清空旧选项
        ClearOptions();
        // 2. 激活选项面板
        optionsGroup.gameObject.SetActive(true);

        // 3. 遍历所有选项（取消数量限制）
        for (int i = 0; i < options.Length; i++)
        {
            // 跳过空文本的选项
            if (string.IsNullOrEmpty(options[i].optionText)) continue;

            // 4. 实例化新按钮（不复用，避免组件缺失）
            GameObject btnObj = Instantiate(optionButtonPrefab, optionsGroup);
            Button optionBtn = btnObj.GetComponent<Button>();
            TextMeshProUGUI optionText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // 5. 安全检查：Button组件必须存在
            if (optionBtn == null)
            {
                Debug.LogError($"选项{i + 1}预制体缺少Button组件！");
                Destroy(btnObj);
                continue;
            }

            // 6. 设置选项文本
            if (optionText != null)
            {
                optionText.text = options[i].optionText;
            }

            // 7. 修复闭包陷阱：捕获当前选项的节点
            DialogueSO currentNextNode = options[i].nextDialogueNode;
            // 8. 绑定点击事件（确保100%生效）
            optionBtn.onClick.RemoveAllListeners();
            optionBtn.onClick.AddListener(() =>
            {
                Debug.Log($"选项{i + 1}点击触发，目标节点：{currentNextNode?.name ?? "空"}");
                OnOptionSelected(currentNextNode);
            });

            Debug.Log($"选项{i + 1}绑定成功，目标节点：{currentNextNode?.name ?? "空"}");
        }
    }

    private void OnOptionSelected(DialogueSO next)
    {
        // 新增：确认方法是否被调用
        Debug.Log($"===== 进入OnOptionSelected ======");
        Debug.Log($"传入的节点：{next?.name ?? "空"}");

        if (next == null)
        {
            Debug.Log("节点为空，执行EndDialogue");
            EndDialogue();
            return;
        }

        Debug.Log($"切换到节点：{next.name}，重置索引为0");
        currentDialogueNode = next;
        currentDialogueIndex = 0;
        LoadDialogueNodeData();
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionsGroup)
        {
            Destroy(child.gameObject); // 销毁而非隐藏，彻底清空
        }
    }

    private void HideOptions()
    {
        optionsGroup.gameObject.SetActive(false);
    }

    private IEnumerator TypeDialogueText(string text)
    {
        isTyping = true;
        dialogueContentText.text = "";
        foreach (char c in text)
        {
            dialogueContentText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void StopTypingCoroutine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }
    }

    private void ShowDialogueUI()
    {
        npcPortrait.gameObject.SetActive(true);
        NPCNameRoot.SetActive(true);
        dialogueContentRoot.SetActive(true);
    }

    private void HideDialogueUI()
    {
        npcPortrait.gameObject.SetActive(false);
        NPCNameRoot.SetActive(false);
        dialogueContentRoot.SetActive(false);
        HideOptions();
    }
}