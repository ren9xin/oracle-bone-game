using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target;
    public bool Locked=true;//摄像机是否跟踪玩家

    [Header("边界限制")]
    public float MinX;
    public float MaxX;
    public float MinY;
    public float MaxY;

    [Header("相机平滑")]
    public float smoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private float z;

    void Awake()
    {
        z = transform.position.z;
    }

    void LateUpdate()
    {
        if (!target) return;
        if(!Locked) return;
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, z);

        // 纯平滑，不做像素对齐，彻底解决卡顿感
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        // 边界 clamped
        smoothed.x = Mathf.Clamp(smoothed.x, MinX, MaxX);
        smoothed.y = Mathf.Clamp(smoothed.y, MinY, MaxY);

        transform.position = smoothed;
    }
}
