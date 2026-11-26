using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // 直接拖入全局不销毁的UI
    public GameObject bagUI;
    public GameObject settingsUI;

    private bool isBagOpen = false;
    private bool isSettingsOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 一开始全部关闭
        CloseAllUI();
    }

    void Update()
    {
        // 对话时禁止操作
        if (对话管理器.Instance != null && 对话管理器.Instance.isInDialogue)
            return;

        if (Input.GetKeyDown(KeyCode.I))
            ToggleBag();

        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleSettings();
    }

    public void ToggleBag()
    {
        isBagOpen = !isBagOpen;
        bagUI.SetActive(isBagOpen);
    }

    public void ToggleSettings()
    {
        isSettingsOpen = !isSettingsOpen;
        settingsUI.SetActive(isSettingsOpen);
    }

    public void CloseAllUI()
    {
        isBagOpen = false;
        isSettingsOpen = false;
        bagUI.SetActive(false);
        settingsUI.SetActive(false);
    }
}