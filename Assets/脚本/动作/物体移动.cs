using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 物体移动 : MonoBehaviour
{
    public Transform targetTrans;//目标位置
    public float totalTime;//移动时间
    public void MoveTo()
    {
        StartCoroutine(MoveCoroutine(transform, targetTrans.position, totalTime));
    }


    // 协程：匀速移动
    private IEnumerator MoveCoroutine(Transform trans, Vector3 targetPos, float totalTime)
    {
        Vector3 startPos = trans.position;
        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            // 匀速插值
            float t = elapsedTime / totalTime;
            trans.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        // 最后精准对齐
        trans.position = targetPos;
    }
}
