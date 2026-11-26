using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadManager : MonoBehaviour
{
    public static LoadManager Instance;
    public GameObject LoadScene;
    public Slider slider;
    public TextMeshProUGUI text;
    private void Awake()
    {
        // 保证全局唯一，不重复、不丢失
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

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        LoadScene.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync("序章");
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            slider.value = operation.progress;
            text.text = operation.progress * 100 + "%";
            if(operation.progress>=0.9f)//allowSceneActivation设为false时进度最多为0.9
            {
                slider.value = 1;
                text.text = "Press AnyKey to continue";
                if(Input.anyKeyDown)
                {
                    operation.allowSceneActivation=true;
                }
            }
            yield return null;
        }

    }
}
