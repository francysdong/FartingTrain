using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("追踪目标")]
    public Transform target;                // 拖入角色

    [Header("追踪设置")]
    public float smoothSpeed = 5f;          // 越大越跟手
    public Vector2 offset;                  // 摄像机偏移

    [Header("边界限制")]
    public bool useBounds = false;          // 是否限制摄像机范围
    public Vector2 minBounds;
    public Vector2 maxBounds;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z          // 保持 Z 轴不变
        );

        // 平滑追踪
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // 边界限制
        if (useBounds)
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
                Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
                transform.position.z
            );
        }
    }
}