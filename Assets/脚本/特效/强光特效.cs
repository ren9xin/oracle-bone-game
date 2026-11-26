using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 强光特效 : MonoBehaviour
{
    Animator ani;
    // Start is called before the first frame update
    void Start()
    {
        ani = GetComponent<Animator>();
    }

    void Light()
    {
        ani.SetTrigger("Start");
        // 延迟2秒（动画时长）后切换场景
        StartCoroutine(SwitchSceneAfterAnimation(1f));
    }

    // 延迟切换场景的协程
    IEnumerator SwitchSceneAfterAnimation(float delayTime)
    {
        yield return new WaitForSeconds(delayTime); // 等动画播放完
        if (场景加载器.Instance != null)
        {
            场景加载器.Instance.TransitionToScene("次元空间");
        }
    }
}
