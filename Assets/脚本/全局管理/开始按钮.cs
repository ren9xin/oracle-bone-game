using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class 开始按钮 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Animator animator;
    // 玩家背包数据（物品列表）
    public Inventory myBag;

    void Start()
    {

    }


    //public void OnBeingDrag(PointerEventData eventData)//开始拖拽
    //{

    //}

    //public void OnDrag(PointerEventData eventData)//拖拽时每一帧
    //{

    //}

    //public void OnEndDrag(PointerEventData eventData)//结束拖拽
    //{

    //}



    //public void OnPointerDown(PointerEventData eventData)//鼠标按下
    //{

    //}
    //public void OnPointerUp(PointerEventData eventData)//鼠标抬起
    //{

    //}


    // Update is called once per frame

    public void OnPointerEnter(PointerEventData eventData)//鼠标进入
    {
        animator.SetBool("选中", true);
    }
    public void OnPointerExit(PointerEventData eventData)//鼠标退出
    {
        animator.SetBool("选中", false);
    }

    public void OnPointerClick(PointerEventData eventData)//点击
    {
        OnStartButtonClick();
    }
    public void OnStartButtonClick()// 按钮点击的触发方法（绑定到UI按钮的OnClick事件）
    {
        // 1. 立即触发动画
        animator.SetTrigger("点击");

        // 2. 启动协程，延迟执行后续逻辑
        StartCoroutine(DelayToStartGame(0.23f));
    }

    // 协程方法：延迟指定时间后执行游戏启动逻辑
    IEnumerator DelayToStartGame(float delayTime)
    {
        // 等待指定的延迟时间（秒）
        yield return new WaitForSeconds(delayTime);

        // 延迟结束后执行的逻辑
        //场景加载器.Instance.TransitionToScene("序章");
        LoadManager.Instance.LoadNextLevel();
        myBag.itemList.Clear();
    }

}
