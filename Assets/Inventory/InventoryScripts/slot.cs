using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class slot : MonoBehaviour
{
    public 物品 slotItem;
    public Image slotImage;
    public TextMeshProUGUI slotNum;

    public void ItemOnclicked()
    {
        InventoryManager.Onclicked(slotItem);
        InventoryManager.UpdateItemInfo(slotItem.itemInfo);
    }
}
