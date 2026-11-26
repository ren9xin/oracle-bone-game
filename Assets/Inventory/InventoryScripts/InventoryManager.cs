using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 背包管理器：负责生成物品格子、刷新UI、显示物品信息、删除物品
public class InventoryManager : MonoBehaviour
{
    // 单例实例，全局唯一，方便其他脚本调用
    public static InventoryManager instance;

    // 玩家背包数据（物品列表）
    public Inventory myBag;

    // 物品格子父物体（所有格子都放在它下面）
    public GameObject slotGrid;

    // 物品格子预制体（用来生成每一个物品格子）
    public slot slotPrefab;

    // 物品描述文本框（显示选中物品的说明）
    public TextMeshProUGUI itemInformation;

    // 当前选中的物品（用于删除、查看详情）
    public 物品 NowSlot;

    // 单例初始化：保证全局只有一个InventoryManager
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // 重复的管理器直接销毁
        }
    }

    // 打开背包时执行：刷新物品、清空描述文本
    private void OnEnable()
    {
        RefreshItem();
        instance.itemInformation.text = "";
    }

    // 更新物品描述信息（外部调用）
    public static void UpdateItemInfo(string itemDescription)
    {
        instance.itemInformation.text = itemDescription;
    }

    // 创建新物品格子：根据物品数据生成UI格子
    public static void CreateNewItem(物品 item)
    {
        // 实例化格子预制体
        slot newItem = Instantiate(instance.slotPrefab, instance.slotGrid.transform.position, Quaternion.identity);

        // 设置格子父物体
        newItem.gameObject.transform.SetParent(instance.slotGrid.transform);

        // 绑定物品数据
        newItem.slotItem = item;

        // 设置物品图标
        newItem.slotImage.sprite = item.itemImage;

        // 如果物品数量为0，自动设置为1（防止显示错误）
        if (item.itemHeld == 0)
        {
            item.itemHeld = 1;
        }

        // 设置物品数量文本
        newItem.slotNum.text = item.itemHeld.ToString();
    }

    // 刷新整个背包UI：清空旧格子 → 重新生成所有物品
    public static void RefreshItem()
    {
        // 销毁所有旧格子
        for (int i = 0; i < instance.slotGrid.transform.childCount; i++)
        {
            Destroy(instance.slotGrid.transform.GetChild(i).gameObject);
        }

        // 重新生成背包里的所有物品
        for (int i = 0; i < instance.myBag.itemList.Count; i++)
        {
            CreateNewItem(instance.myBag.itemList[i]);
        }
    }

    // 点击物品时记录当前选中的物品
    public static void Onclicked(物品 item)
    {
        instance.NowSlot = item;
    }

    // 删除当前选中的物品：数量清零、从列表移除、清空选中、刷新UI
    public static void Destroy()
    {
        if (instance.NowSlot != null)
        {
            instance.NowSlot.itemHeld = 0;          // 物品数量归零
            instance.myBag.itemList.Remove(instance.NowSlot); // 从背包移除
            instance.NowSlot = null;                 // 清空当前选中
            RefreshItem();                           // 刷新UI
            instance.itemInformation.text = "";      // 清空描述
        }
    }

    // ============================================================
    // 剧情/系统接口：让对白 callback 或编排脚本能直接给背包加物品
    // ============================================================

    // 把物品加入背包（去重）。背包 UI 若打开则即时刷新。
    public static void 添加物品(物品 item)
    {
        if (item == null || instance == null || instance.myBag == null) return;
        if (instance.myBag.itemList.Contains(item))
        {
            Debug.Log("↺ 背包已有：" + item.itemName + "，跳过");
            return;
        }
        instance.myBag.itemList.Add(item);
        if (item.itemHeld <= 0) item.itemHeld = 1;
        Debug.Log("✅ 背包添加：" + item.itemName);
        if (instance.slotGrid != null && instance.slotGrid.activeInHierarchy)
            RefreshItem();
    }

    // 强制刷新当前打开的背包 UI（演化动画完成后调用）
    public static void 强制刷新UI()
    {
        if (instance == null || instance.slotGrid == null) return;
        if (instance.slotGrid.activeInHierarchy) RefreshItem();
    }
}
