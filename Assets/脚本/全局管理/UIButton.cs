using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    public enum BtnType { 打开背包, 关闭背包, 打开设置, 关闭设置, 存档, 读档, 丢弃, 重新开始, 退出游戏 }
    public BtnType type;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        switch (type)
        {
            case BtnType.打开背包: UIManager.Instance.ToggleBag(); break;
            case BtnType.关闭背包: UIManager.Instance.ToggleBag(); break;

            case BtnType.打开设置: UIManager.Instance.ToggleSettings(); break;
            case BtnType.关闭设置: UIManager.Instance.ToggleSettings(); break;

            case BtnType.存档: GameSaveManager.Instance.SaveGame(); break;
            case BtnType.读档: GameSaveManager.Instance.LoadGame(); break;

            case BtnType.重新开始: GameManager.Instance.PlayerRestartCurrentScene(); break;

            case BtnType.退出游戏:
                Debug.Log("点击退出游戏按钮，准备退出游戏...");
#if UNITY_EDITOR
                // Unity 编辑器中：停止游戏运行
                UnityEditor.EditorApplication.isPlaying = false;
#else
                // 真机环境：退出应用程序
                Application.Quit();
#endif
                break;

                //case BtnType.丢弃: InventoryManager.instance.Destroy(); break;
        }
    }
}
