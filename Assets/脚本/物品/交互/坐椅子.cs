using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 坐椅子 : MonoBehaviour
{
    角色脚本 player;
    private bool isPlayerInRange = false; // 玩家是否在交互范围内
    Animator ani;
    // 引用目标物体的SpriteRenderer组件（也可以动态获取）
    public SpriteRenderer targetSpriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        player=GetComponent<角色脚本>();
        ani=GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInRange)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                targetSpriteRenderer.enabled = false;
                ani.SetTrigger("isSit");
                player.MoveBaned();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("chair"))
        {
            isPlayerInRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("chair"))
        {
            isPlayerInRange = false;
        }
    }
}
