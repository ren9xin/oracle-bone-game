using System.Collections;
using UnityEngine;
using TMPro;

public class 金币UI : MonoBehaviour
{
    public int targetPlayerID;
    public GameObject targetPlayer;
    public TextMeshProUGUI moneyText;

    void Start()
    {
        restart();
    }

    public void restart()
    {
        StartCoroutine(DelayBindPlayer());
    }

    IEnumerator DelayBindPlayer()
    {
        yield return null;

        if (targetPlayer == null) yield break;
        角色脚本 player = targetPlayer.GetComponent<角色脚本>();
        if (player == null) yield break;

        player.money = this;

        GameManager.Instance.OnDataChanged += RefreshFromGM;
        RefreshFromGM();
    }

    public void RefreshFromGM()
    {
        if (GameManager.Instance == null) return;
        SetMoney(GameManager.Instance.moneyCount);
    }

    public void SetMoney(int moneycount)
    {
        if (moneyText != null)
            moneyText.text = moneycount.ToString();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDataChanged -= RefreshFromGM;
    }
}
