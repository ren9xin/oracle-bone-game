using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 场景退出 : MonoBehaviour
{
    [Tooltip("需要过渡新场景的名称")]
    public string newSceneName;
 
    //调用场景切换函数
    public void TransitionIntenal()
    {
        场景加载器.Instance.TransitionToScene(newSceneName);
    }
}
