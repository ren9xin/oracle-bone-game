using UnityEngine;

// 秦兵 idle 帧的简单巡逻：左右往返走，靠 SpriteRenderer.flipX 做"看左/看右"。
// 当 Codex 的 walk spritesheet 到位后，可换成 Animator 驱动的版本，本组件就不再需要。
public class QinSoldierPatrol : MonoBehaviour
{
    [Header("巡逻范围（围绕初始 X 位置左右各走多远）")]
    public float 巡逻半径 = 3f;
    public float 移动速度 = 1f;

    [Header("初始朝向（true=向右，false=向左）")]
    public bool 初始朝右 = true;

    Vector3 起始位置;
    int 方向 = 1;   // 1=向右, -1=向左
    SpriteRenderer sr;

    void Start()
    {
        起始位置 = transform.position;
        方向 = 初始朝右 ? 1 : -1;
        sr = GetComponent<SpriteRenderer>();
        刷新朝向();
    }

    void Update()
    {
        transform.Translate(Vector3.right * 方向 * 移动速度 * Time.deltaTime, Space.World);
        float dx = transform.position.x - 起始位置.x;
        if (dx > 巡逻半径 && 方向 == 1) { 方向 = -1; 刷新朝向(); }
        else if (dx < -巡逻半径 && 方向 == -1) { 方向 = 1; 刷新朝向(); }
    }

    void 刷新朝向()
    {
        if (sr != null) sr.flipX = (方向 == -1);
    }
}
