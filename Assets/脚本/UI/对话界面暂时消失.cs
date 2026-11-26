using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class 对话界面暂时消失 : MonoBehaviour
{
    public float time = 3;
    Canvas canvas;

    void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    public void disappear()
    {
        StartCoroutine(Dis());
    }

    public IEnumerator Dis()
    {
        canvas.enabled = false; // 关闭画布，不关闭物体
        yield return new WaitForSeconds(time);
        canvas.enabled = true;
    }
}
