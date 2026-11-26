using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 捡箱子 : MonoBehaviour
{
    public bool isInRange;
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
        if (isInRange)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Pass.PassAllow = true;
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag=="Player")
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag=="Player")
        {
            isInRange = false;
        }
    }
}
