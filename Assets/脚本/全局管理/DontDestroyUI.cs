using UnityEngine;

public class DontDestroyUI : MonoBehaviour
{
    void Awake()
    {
        // 让整个UI根节点跨场景不销毁
        DontDestroyOnLoad(gameObject);
    }
}
