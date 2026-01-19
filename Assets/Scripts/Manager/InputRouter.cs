using UnityEngine;
using System;

public class InputRouter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlow; // 게임 상태 참조

    // 상태별 탭 이벤트 (다음 챕터에서 구독해서 사용)
    public event Action OnTapReady;
    public event Action OnTapFlying;
    public event Action OnTapAny;

    private void Update()
    {
        // WebGL/모바일 대응: 마우스 클릭을 터치처럼 사용
        // (모바일 브라우저에서도 보통 잘 동작)
        if (Input.GetMouseButtonDown(0))
        {
            RouteTap();
        }
    }

    private void RouteTap()
    {
        // 1) 탭 순간 상태 캐싱
        var stateAtTap = gameFlow.CurrentState;

        // 2) 공통 탭 처리 (GameOver에서 Retry 같은 게 여기서 일어남)
        OnTapAny?.Invoke();

        // ✅ 3) OnTapAny에서 상태가 바뀌었다면, 이 탭은 "소비"된 것
        // 즉, 같은 탭으로 Ready/Flying 로직이 이어서 실행되면 안 됨
        if (gameFlow.CurrentState != stateAtTap)
            return;

        // 4) 상태가 그대로면 분기 처리
        switch (stateAtTap)
        {
            case GameFlowManager.GameState.Ready:
                Debug.Log("[InputRouter] Tap -> Ready");
                OnTapReady?.Invoke();
                break;

            case GameFlowManager.GameState.Flying:
                Debug.Log("[InputRouter] Tap -> Flying");
                OnTapFlying?.Invoke();
                break;

            default:
                Debug.Log($"[InputRouter] Tap ignored in state: {stateAtTap}");
                break;
        }
    }

}
