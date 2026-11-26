using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class 爱心 : MonoBehaviour
{
    public ObjectPool<GameObject> pool;
    bool isApproaching;
    int hp = 5;
    bool isCounting;
    角色脚本 player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<角色脚本>();
    }

    private void OnEnable()
    {
        // 每次被唤醒重置状态
        isApproaching = false;
        isCounting = false;
        StopAllCoroutines(); // 杀死残留旧协程
        StartCoroutine(Dead());
    }


    // Update is called once per frame
    void Update()
    {
        if (isApproaching)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (player != null)
                {
                    player.GetHurt(-hp);
                    pool.Release(gameObject);
                }
            }
        }
    }

    IEnumerator Dead()
    {
        if (isCounting) yield break;
        isCounting = true;
        yield return new WaitForSeconds(5f);
        pool.Release(gameObject);
        isCounting=false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isApproaching = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isApproaching = false;
        }
    }
}
