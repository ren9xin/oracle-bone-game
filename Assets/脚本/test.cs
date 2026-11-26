using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
public class test : MonoBehaviour
{
    double startTime;
    int CoolDown = 5;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Trigger());
        }
    }

    IEnumerator Trigger()
    {
        if(startTime+CoolDown>Time.time)
        {
            yield break; 
        }
        startTime = Time.time;
        Debug.Log("Hello");
        yield return null;
    }
}
