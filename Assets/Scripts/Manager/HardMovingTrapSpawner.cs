using UnityEngine;

public class HardMovingTrapSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Spawn Rule (Hard Tier)")]
    [SerializeField] private float startDistance = 250f; // hard 구간 시작 거리(=hardTierDistance)
    [SerializeField] private float spawnEveryDistance = 35f; // 몇 m마다 1개 스폰

    [Header("Spawn Area")]
    [SerializeField] private float spawnAheadX = 35f; // 플레이어 앞쪽 몇 유닛에 생성
    [SerializeField] private float minY = -2f;        // a
    [SerializeField] private float maxY = 6f;         // b

    [Header("Trap Prefab")]
    [SerializeField] private GameObject movingTrapPrefab;

    [Header("Trap Move")]
    [SerializeField] private float trapMoveSpeed = 8f; // n

    private float runStartX;
    private float nextSpawnAtDistance;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        if (player != null) runStartX = player.position.x;

        // 하드 진입 후 첫 스폰 지점 예약
        nextSpawnAtDistance = startDistance;
    }

    private void Update()
    {
        if (player == null || movingTrapPrefab == null) return;

        // 현재 진행 거리 = playerX - runStartX
        float dist = Mathf.Max(0f, player.position.x - runStartX);

        // 1) hard 구간 도달 전이면 스폰 금지
        if (dist < startDistance) return;

        // 일정 거리마다 1개 스폰
        if (dist >= nextSpawnAtDistance)
        {
            SpawnOne();
            nextSpawnAtDistance += spawnEveryDistance;
        }
    }

    private void SpawnOne()
    {
        // 2) a~b 사이 랜덤 Y
        float y = Random.Range(minY, maxY);

        // 플레이어 앞쪽에서 생성
        Vector3 pos = new Vector3(player.position.x + spawnAheadX, y, 0f);

        GameObject go = Instantiate(movingTrapPrefab, pos, Quaternion.identity);

        // 3) 좌측 이동 속도 세팅
        var mt = go.GetComponent<MovingTrap>();
        if (mt != null)
            mt.Init(player, trapMoveSpeed);
    }

    /// <summary>
    /// Retry에서 런 기준점 재설정(거리 기준 리셋용)
    /// </summary>
    public void ResetRun()
    {
        if (player != null) runStartX = player.position.x;
        nextSpawnAtDistance = startDistance;
    }

    public void SetPlayer(Transform t)
    {
        player = t;
    }
}
