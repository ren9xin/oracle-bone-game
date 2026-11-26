using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 捡金币 : MonoBehaviour
{
    public int money=5;
    bool isApproaching;
    角色脚本 player;
    void Start()
    {
        player= GameObject.FindGameObjectWithTag("Player").GetComponent<角色脚本>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isApproaching)
        {
            if (Input.GetKeyDown(KeyCode.F)) {
                if (player != null)
                {
                    player.GetMoney(money);
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            isApproaching = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            isApproaching = false;
        }
    }
}
