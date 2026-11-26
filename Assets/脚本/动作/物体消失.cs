using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 物体消失 : MonoBehaviour
{
    void disappear()
    {
        gameObject.SetActive(false);
    }
}
