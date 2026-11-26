using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class 场景加载器 : MonoBehaviour
{
    public static 场景加载器 Instance {  get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);//加载新场景时告诉游戏引擎，不要销毁该对象
    }
    
    //切换场景函数
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    //切换场景协程
    public IEnumerator TransitionCoroutine(string newSceneName)
    {
        //保持所有持久化数据

        //读出当前场景

        //异步加载新场景
        yield return SceneManager.LoadSceneAsync(newSceneName);
        //加载所有持久化数据

        //获取目标场景过渡的位置

        //设置进入游戏对象的位置

        //淡入新场景
    }
}
