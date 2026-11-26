using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName="New Inventory",menuName="Inventory/New Inventory")]
public class Inventory : ScriptableObject
{
    public List<物品>itemList=new List<物品>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //测试用
    public void AddItem(物品 item)
    {
        if (!itemList.Contains(item))
        {
            itemList.Add(item);
            if (item.itemHeld <= 0) item.itemHeld = 1;
        }
    }

    public void RemoveItem(物品 item)
    {
        if (itemList.Contains(item))
        {
            itemList.Remove(item);
        }
    }

    public bool HasItem(物品 item) => itemList.Contains(item);
}
