using UnityEngine;
using TMPro;

public class RunStatsUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RunStatsManager stats;

    [Header("UI (TMP)")]
    [SerializeField] private TMP_Text txtDistance;
    [SerializeField] private TMP_Text txtBest;

    private void Update()
    {
        if (stats == null) return;

        // 표시 포맷(Format, 포맷): 소수점 버림, m 단위 표기
        int cur = Mathf.FloorToInt(stats.CurrentDistance);
        int best = Mathf.FloorToInt(stats.BestDistance);

        if (txtDistance != null) txtDistance.text = $"{cur}m";
        if (txtBest != null) txtBest.text = $"BEST {best}m";
    }
}
