using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 爬梯子 : MonoBehaviour
{
    public float vertical;//垂直速度
    public float speed = 3f;
    private bool isLadder;//是否在梯子上
    public bool isClimbing;//是否在爬梯子


    Animator anim;
    [SerializeField] Rigidbody2D rb;
    public float originalGravity;

    角色脚本 player;
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<角色脚本>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerInputLock.inputLocked) return;
        vertical = Input.GetAxis("Vertical");

        if (isLadder && Mathf.Abs(vertical) > 0)
        {
            isClimbing = true;
        }

        // 按空格 → 取消攀爬，跳出去
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isClimbing = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = true;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimbing = false;
            anim.speed = 1f;
        }
    }

    private void FixedUpdate()
    {
        if (PlayerInputLock.inputLocked) return;
        if (isClimbing)
        {
            // ==========================================
            //  核心：有输入 → 动；没输入 → 停
            // ==========================================
            bool hasVerticalInput = Mathf.Abs(vertical) > 0.1f;

            if (hasVerticalInput)
            {
                // 有按上下 → 正常播放动画
                anim.speed = 1;
                anim.SetBool("Climbing", true);
                rb.velocity = new Vector2(rb.velocity.x, vertical * speed);
            }
            else
            {
                // 没按上下 → 暂停动画
                anim.speed = 0;
                rb.velocity = new Vector2(rb.velocity.x, 0); // 停在原地
            }

            rb.gravityScale = 0;
            // 爬梯子时允许跳跃（假装在地面）
            if (player != null)
                player.m_grounded = true;
        }
        else
        {
            rb.gravityScale = originalGravity;
            anim.SetBool("Climbing", false);
        }

    }
}
