using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 摄像机移动 : MonoBehaviour
{
    public CameraControl camera;
    public Transform targetTrans;//目标位置
    public float totalTime;//移动时间
    // Start is called before the first frame update
    void Start()
    {
        camera=gameObject.GetComponent<CameraControl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Move()
    {
        if (targetTrans != null)
        {
            camera.Locked = false;
            StartCoroutine(MoveCoroutine(transform, targetTrans.position, totalTime));
        }
    }

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
