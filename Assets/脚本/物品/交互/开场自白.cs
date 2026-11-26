using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 开场自白 : MonoBehaviour
{
    public DialogueSO startDialogue;
    public PlayerGravitySetter playerGravitySetter;
    public PlayerInputLock playerInputLock;
    // Start is called before the first frame update
    void Start()
    {
        if (startDialogue != null)
        {
            对话管理器.Instance.StartDialogue(startDialogue);
        }

        if (playerGravitySetter != null)
        {
            playerGravitySetter.SetTargetGravity();
        }

        if (playerInputLock != null)
        {
            playerInputLock.LockInput();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
