using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 地图物品 : MonoBehaviour
{
    public 物品 thisItem;
    public Inventory playerInventory;
    // 标记玩家是否在可拾取范围内
    private bool canPickup = false;

    // 是否全局唯一（勾选=只能有1个）
    public bool isUnique;
    void Start()
    {
        // 如果是唯一物品，且玩家已经有了 → 直接消失
        if (isUnique && playerInventory.itemList.Contains(thisItem))
        {
            Destroy(gameObject);
        }
    }
    // 每一帧都执行，用于检测输入
    private void Update()
    {
        // 只有在可拾取状态下，检测F键按下
        if (canPickup && Input.GetKeyDown(KeyCode.F))
        {
            AddNewItem(); // 执行添加道具逻辑
            Destroy(gameObject); // 销毁道具对象
            canPickup = false; // 重置状态（可选）
        }
    }

    // Update is called once per frame

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测碰撞对象是否是玩家
        if (collision.CompareTag("Player"))
        {
            canPickup = true; // 开启可拾取状态
        }
    }

    // 当玩家离开碰撞框时触发（只执行一次）
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 检测碰撞对象是否是玩家
        if (collision.CompareTag("Player"))
        {
            canPickup = false; // 关闭可拾取状态
        }
    }

    

    void AddNewItem()
    {
        if (!playerInventory.itemList.Contains(thisItem))
        {
            playerInventory.itemList.Add(thisItem);
            // 唯一物品：强制数量=1
            if (isUnique)
                thisItem.itemHeld = 1;
            else
                thisItem.itemHeld += 1;
        }
        else
        {
            // 唯一物品：永远保持1
            if (isUnique)
                thisItem.itemHeld = 1;
            else
                thisItem.itemHeld += 1;

        }
        InventoryManager.RefreshItem();
    }
}
