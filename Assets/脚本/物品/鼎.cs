using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("肉预制体")]
    public GameObject meatPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("LightAttack")
     || collision.gameObject.layer == LayerMask.NameToLayer("StrongAttack"))
        {
            Instantiate(meatPrefab, transform.position, Quaternion.identity);
            // 自己消失（销毁自己）
            Destroy(gameObject);
        }
    }
}
