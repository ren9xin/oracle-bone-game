using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "SO/对话数据", order = 2)]
public class DialogueSO : ScriptableObject
{
    [Header("对话关联信息")]
    public NPCSO npcData;

    [Header("对话内容")]
    [TextArea(3, 10)]
    public string[] dialogueContent;

    [Header("对话分支选项")]
    public DialogueOption[] dialogueOptions;

    [Header("下一个对话节点")]
    public DialogueSO NextDialogueNode;

    [Header("对话结束回调（可多个同时执行）")]
    public DialogueCallback[] callbacks;

    [Serializable]
    public class DialogueCallback
    {
        public string targetObjectName;  // 物体名
        public string callbackFunctionName; // 函数名
    }

    [Serializable]
    public class DialogueOption
    {
        public string optionText;
        public DialogueSO nextDialogueNode;
    }
}