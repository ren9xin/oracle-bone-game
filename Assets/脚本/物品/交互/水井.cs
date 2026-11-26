using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 水井 : MonoBehaviour
{
    private bool isPlayerInRange = false; // 玩家是否在交互范围内
    public string nextScene;//切换的场景
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInRange)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                场景加载器.Instance.TransitionToScene(nextScene);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
