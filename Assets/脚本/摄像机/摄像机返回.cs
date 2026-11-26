using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 摄像机返回 : MonoBehaviour
{
    public CameraControl camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = gameObject.GetComponent<CameraControl>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Return()
    {
        if (camera != null)
        {
            camera.Locked = true;
        }
    } 
}
