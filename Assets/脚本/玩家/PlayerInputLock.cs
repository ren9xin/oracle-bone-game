using UnityEngine;

public class PlayerInputLock : MonoBehaviour
{
    public static bool inputLocked = false;

    void Start()
    {
        // 场景启动时锁定输入
        LockInput();
    }

    public void LockInput()
    {
        inputLocked = true;
    }

    public void UnlockInput()
    {
        inputLocked = false;
    }
}
