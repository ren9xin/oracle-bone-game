using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 传送门 : MonoBehaviour
{
    [Header("这是第几号门")]
    public int gateIndex = 1;

    [Tooltip("严格匹配：true 时只有 unlockedGateIndex == gateIndex 才能进（次元空间 4 门用，玩家不能回头）。false 时 >= 即可（监狱等旧场景用）。")]
    public bool 严格匹配 = false;

    [Header("动画")]
    public Animator animator;
    public string openParam = "IsOpen";    // 可进入动画参数

    private bool canEnter;

    private bool isPlayerInRange = false; // 玩家是否在交互范围内
    public string nextScene;//切换的场景

    [Header("进入前需要完成的 UI 挑战（可选）")]
    [Tooltip("非空时，玩家按 F 不会立即跳场景，而是激活此 GameObject（例如暗号拼字 Canvas）。UI 内部完成挑战后自行调 TransitionToScene。")]
    public GameObject 进入前面板;
    // Start is called before the first frame update
    private void Awake()
    {
        // 一出生就监听解锁事件
        GameManager.Instance.OnGateUnlocked += RefreshGateState;
    }
    void Start()
    {
        RefreshGateState();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInRange)
        {
            if (canEnter)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // 注意：不要在此调 UnlockNextGate。解锁推进由各章末尾的剧情脚本负责。
                    if (进入前面板 != null && !进入前面板.activeSelf)
                    {
                        // 该门需要先在 UI 里完成挑战（例如暗号拼字）→ 弹面板，由面板自己跳场景
                        进入前面板.SetActive(true);
                    }
                    else
                    {
                        场景加载器.Instance.TransitionToScene(nextScene);
                    }
                }
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
    // 收到事件就自动刷新自己
    //   - 严格匹配 = true（次元空间 4 门）：unlockedGateIndex == gateIndex 才允许进入，不能回头
    //   - 严格匹配 = false（监狱等其他场景的传送门）：unlockedGateIndex >= gateIndex 即可
    public void RefreshGateState()
    {
        int idx = GameManager.Instance.unlockedGateIndex;
        canEnter = 严格匹配 ? (idx == gateIndex) : (idx >= gateIndex);

        if (animator != null)
        {
            animator.SetBool(openParam, canEnter);
        }
    }

}
