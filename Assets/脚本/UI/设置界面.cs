using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 设置界面 : MonoBehaviour
{
    static 设置界面 Instance;
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
}
