using UnityEngine;

public class RunStatsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;  // 추적 대상(플레이어)

    [Header("Stats")]
    [SerializeField] private float currentDistance; // 현재 거리(미터처럼 쓰기)
    [SerializeField] private float bestDistance;

    private float runStartX;

    private const string KEY_BEST = "BEST_DISTANCE";

    public float CurrentDistance => currentDistance;
    public float BestDistance => bestDistance;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 최고 기록 로드
        bestDistance = PlayerPrefs.GetFloat(KEY_BEST, 0f);
    }

    private void Update()
    {
        if (player == null) return;

        // 거리 갱신
        currentDistance = Mathf.Max(0f, player.position.x - runStartX);

        // 실시간 최고 기록 갱신(원하면 GameOver에서만 갱신해도 됨)
        if (currentDistance > bestDistance)
        {
            bestDistance = currentDistance;
            PlayerPrefs.SetFloat(KEY_BEST, bestDistance);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 런 시작(Ready로 돌아갈 때) 기준점 초기화
    /// </summary>
    public void BeginRun()
    {
        if (player == null) return;

        runStartX = player.position.x;
        currentDistance = 0f;
    }

    /// <summary>
    /// 필요 시 외부에서 플레이어 재할당
    /// </summary>
    public void SetPlayer(Transform t)
    {
        player = t;
    }
}
