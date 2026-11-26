using UnityEngine;
using System.Collections;

public class 隐身 : MonoBehaviour
{
    [Header("透明度设置")]
    public float fadeAlpha = 0.4f; // 透明程度
    public float fadeDuration = 0.8f; // 透明持续时间

    [Header("冷却")]
    public float cooldown = 2f;

    [Header("触发按键")]
    public KeyCode triggerKey = KeyCode.E;

    private SpriteRenderer sprite;
    private float cooldownTimer;
    public bool isFaded;

    void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        cooldownTimer = cooldown + 1f;
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (PlayerInputLock.inputLocked) return;
        if (Input.GetKeyDown(triggerKey) && cooldownTimer >= cooldown && !isFaded)
        {
            StartCoroutine(DoFade());
        }
    }

    IEnumerator DoFade()
    {
        isFaded = true;
        cooldownTimer = 0;

        // 变透明
        SetAlpha(fadeAlpha);

        // 保持透明一段时间
        yield return new WaitForSeconds(fadeDuration);

        // 恢复不透明
        SetAlpha(1f);

        isFaded = false;
    }

    void SetAlpha(float alpha)
    {
        if (sprite != null)
        {
            Color c = sprite.color;
            c.a = alpha;
            sprite.color = c;
        }
    }
}
