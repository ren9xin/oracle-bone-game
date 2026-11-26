using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 字典面板里的一个槽。挂在槽预制体根节点上。
// 把 Button.OnClick 在 Inspector 里指到本脚本的 被点击() 方法。
public class 字典槽 : MonoBehaviour
{
    public 甲骨字 字;
    public Image 字形显示;
    public TextMeshProUGUI 现代字显示;

    public void 绑定(甲骨字 新字)
    {
        字 = 新字;
        if (字 == null) return;
        if (字形显示 != null) 字形显示.sprite = 字.字形图;
        if (现代字显示 != null) 现代字显示.text = 字.现代字;
    }

    public void 被点击()
    {
        if (字典管理器.Instance != null) 字典管理器.Instance.选中(字);
    }
}
