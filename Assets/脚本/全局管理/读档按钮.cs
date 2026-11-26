using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// 开始界面"读档"按钮 —— 模仿 开始按钮.cs 的 hover/click 动画 pattern。
// 鼠标进入：animator.SetBool("选中", true) → 高亮
// 鼠标点击：animator.SetTrigger("点击") → 延迟 0.23s → 调 GameSaveManager.LoadGame()
public class 读档按钮 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Animator animator;
    [Tooltip("点击动画播放完再执行读档的延迟（秒）。")]
    public float 延迟秒 = 0.23f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator != null) animator.SetBool("选中", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null) animator.SetBool("选中", false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnLoadButtonClick();
    }

    public void OnLoadButtonClick()
    {
        if (animator != null) animator.SetTrigger("点击");
        StartCoroutine(DelayLoad());
    }

    IEnumerator DelayLoad()
    {
        yield return new WaitForSeconds(延迟秒);
        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.LoadGame();
        else
            Debug.LogWarning("读档按钮: GameSaveManager.Instance 为空，无法读档");
    }
}
