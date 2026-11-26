using UnityEngine;

public class DontDestroySingleton : MonoBehaviour
{
    public static DontDestroySingleton Instance;

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
