using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // 따라갈 대상(Player)

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.15f; // 따라가는 부드러움(작을수록 빨리 따라감)
    [SerializeField] private Vector2 offset = new Vector2(2.0f, 1.0f); // 카메라 오프셋(우측/상단)

    [Header("Clamp (Optional)")]
    [SerializeField] private bool clampY = false; // Y 고정/제한 여부
    [SerializeField] private float minY = -2.0f;
    [SerializeField] private float maxY = 10.0f;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 = 타겟 + 오프셋 (Z는 카메라 고정)
        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        if (clampY)
        {
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        // SmoothDamp: 흔들림 적고 자연스러운 추적
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

}
