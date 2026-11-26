using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    [Header("巡逻")]
    public float moveSpeed = 2f;
    public float patrolRange = 3f;
    public float waitTimeAtEdge = 2f; // 边界停顿时间

    [Header("动画")]
    public Animator anim;

    private Vector2 startPos;
    public int faceDir = 1;
    public bool isChasing = false;
    public bool isReturning = false;
    private bool isWaiting = false;

    void Start()
    {
        startPos = transform.position;
        if (anim == null)
            anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isChasing)
        {
            anim.SetBool("IsMove", false);
            //anim.SetBool("IsRun", false);// 关闭跑步动画（防止残留）
            return;
        }

        // 回家模式
        if (isReturning)
        {
            ReturnToStart();
            return;
        }

        if (!isWaiting)
        {
            Patrol();
        }
    }

    void Patrol()
    {
        // 移动
        transform.Translate(Vector2.right * faceDir * moveSpeed * Time.deltaTime);
        anim.SetBool("IsMove", true);

        // 判断是否到边界
        float distFromStart = Mathf.Abs(transform.position.x - startPos.x);
        if (distFromStart >= patrolRange)
        {
            // 停住，播放待机动画
            anim.SetBool("IsMove", false);
            StartCoroutine(WaitAndFlip());
        }
    }

    IEnumerator WaitAndFlip()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtEdge); // 等待N秒
        Flip(); // 掉头
        transform.Translate(Vector2.right * faceDir * moveSpeed * 0.1f);
        isWaiting = false;
    }

    // 统一翻转逻辑
    public void Flip()
    {
        faceDir *= -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * faceDir;
        transform.localScale = scale;
    }

    public void StartChase() => isChasing = true;

    // 脱战 → 先等待再回家
    public void StopChase()
    {
        isChasing = false;
        StopAllCoroutines();
        isWaiting = false;
        anim.SetBool("IsRun", false);

        // 先等待 → 再开启回家
        StartCoroutine(WaitThenReturn());
    }

    // 先等待，再回家
    IEnumerator WaitThenReturn()
    {
        isWaiting = true;
        anim.SetBool("IsMove", false);
        yield return new WaitForSeconds(waitTimeAtEdge); // 站立等待
        isWaiting = false;

        // 朝向出生点
        Vector2 dir = startPos - (Vector2)transform.position;
        int targetDir = (int)Mathf.Sign(dir.x);

        if (targetDir != faceDir)
        {
            Flip();
        }

        // 开始回家
        isReturning = true;
    }

    // 回家逻辑
    void ReturnToStart()
    {
        transform.Translate(Vector2.right * faceDir * moveSpeed * Time.deltaTime);
        anim.SetBool("IsMove", true);

        if (Vector2.Distance(transform.position, startPos) < 0.15f)
        {
            isReturning = false;
        }
    }

}
