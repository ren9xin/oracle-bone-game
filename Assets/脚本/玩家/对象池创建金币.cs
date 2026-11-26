using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class 对象池创建爱心 : MonoBehaviour
{
    public GameObject heart;
    public ObjectPool<GameObject> pool;

    void Start()
    {
        pool = new ObjectPool<GameObject>(CreateHeart,GetHeart,ReleaseHeart,DestroyHeart);
    }

    GameObject CreateHeart()
    {
        var obj=Instantiate(heart);
        obj.GetComponent<爱心>().pool = pool;
        obj.SetActive(false);
        return obj;
    }

    void GetHeart(GameObject obj)
    {
        obj.SetActive(true);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
    }

    void ReleaseHeart(GameObject obj)
    {
        obj.SetActive(false);
    }

    void DestroyHeart(GameObject obj)
    {
        Destroy(obj);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            pool.Get();
        }
    }
}
