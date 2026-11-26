using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class 血条 : MonoBehaviour
{
    public UnityEngine.UI.Slider slider;
    public int targetPlayerID;
    public GameObject targetPlayer;
    public TextMeshProUGUI healthText;
    void Start()
    {
        restart();
    }

    public void restart()
    {
        // 优化1：延迟一帧执行，避开玩家注册与血条初始化的顺序问题
        StartCoroutine(DelayBindPlayer());
    }
    /// <summary>
    /// 延迟绑定玩家，解决执行顺序问题，同时完善判空逻辑
    /// </summary>
    IEnumerator DelayBindPlayer()
    {
        // 延迟0帧（等待当前帧所有Start执行完毕），也可改为 new WaitForSeconds(0.1f) 更稳妥
        new WaitForSeconds(0.1f);
        yield return null;

        //先判空 血条匹配 管理器，避免空指针
        /*if (血条匹配.Instance == null)
        {
            yield break; // 终止协程，不再执行后续逻辑
        }

        // 获取对应ID的玩家
        targetPlayer = 血条匹配.Instance.GetPlayerByID(targetPlayerID);*/
        if (targetPlayer == null) yield break;

        角色脚本 player = targetPlayer.GetComponent<角色脚本>();
        if (player == null) yield break;

        player.healthBar = this;

        GameManager.Instance.OnDataChanged += RefreshFromGM;
        RefreshFromGM();
    }

    public void RefreshFromGM()
    {
        if (GameManager.Instance == null) return;
        SetHealth(GameManager.Instance.currentHealth, GameManager.Instance.maxHealth);
    }

    public void SetHealth(int currenthealth, int maxhealth)
    {
        if (slider != null)
        {   
            slider.maxValue = maxhealth;
            slider.value = currenthealth;
            SetTextHealth(currenthealth, maxhealth);
        }
    }

    public void SetTextHealth(int currenthealth, int maxhealth)
    {
        if (healthText != null)
            healthText.text = currenthealth + "/" + maxhealth;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDataChanged -= RefreshFromGM;
    }

}