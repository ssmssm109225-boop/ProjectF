using UnityEngine;

public class UpperHit : TriggerObjectBase
{
    [Header("Reorient Settings")]
    [SerializeField] private float fixedAngleDeg = 35f; // 항상 이 각도로 우측 발사
    [SerializeField] private float minSpeed = 8f;       // 최소 속도 보장(너무 느릴 때만)
    [SerializeField] private float extraSpeed = 0f;     // 맞을 때 속도 보너스(선택)

    protected override void OnHitGreen(PlayerController player)
    {
        player.ReorientVelocityToAngle(fixedAngleDeg, minSpeed, extraSpeed);
        Debug.Log($"[UpperHit] Reorient angle={fixedAngleDeg}, minSpeed={minSpeed}, extra={extraSpeed}");
    }
}
