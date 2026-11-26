using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 触碰切换场景 : MonoBehaviour
{
    public string targetScene;
    通关判断 Pass;
    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Pass= player.GetComponent<通关判断>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Pass == null||Pass.PassAllow==false)
        {
            return;
        }
        if(collision.tag=="Player")
        {
            if(targetScene!=null)
            {
                场景加载器.Instance.TransitionToScene(targetScene);
            }
            
        }
    }
}
