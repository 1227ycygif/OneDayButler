using UnityEngine;

/// <summary>
/// CatAnimation(FSM)에게 씬 참조만 넘기고 즉시 비활성화되는 레거시 셰임.
/// - NavMesh/로밍/행동 로직은 전부 CatAnimation(FSM)이 처리.
/// - 이 스크립트는 씬 오브젝트 찾기/연결만 담당.
/// </summary>
[DisallowMultipleComponent]
public class CatRoaming : MonoBehaviour
{
    [Header("연결 대상 (FSM)")]
    public CatAnimation cat;                 // CatAnimation(FSM) 컴포넌트 드래그

    [Header("목표 지점들 (NavMesh 위)")]
    public Transform bowlGPS;                // 밥 그릇 앞 바닥 포인트
    public Transform bed;                    // 침대 앞/위 포인트
    public Transform toiletGPS;              // 화장실 앞 바닥 포인트

    [Header("로밍 포인트 루트")]
    public Transform roamingRoot;            // 자식들이 Roaming 포인트

    [Header("선택 연결")]
    public csBowl bowl;                      // 있으면 checkEmpty() 활용
    public csToilet toilet;                  // 있으면 checkEmpty()/poop() 활용
    public CatState catState;                // 있으면 상태 연동

    void Awake()
    {
        if (!cat)
        {
            cat = GetComponent<CatAnimation>();
            if (!cat)
            {
                Debug.LogError("[CatRoamingShim] CatAnimation(FSM) 미지정");
                enabled = false;
                return;
            }
        }

        // FSM에 참조 주입
        cat.bowl = bowl ? bowl : cat.bowl;
        cat.toilet = toilet ? toilet : cat.toilet;
        cat.cat = catState ? catState : cat.cat;

        cat.bowlPoint = bowlGPS ? bowlGPS : cat.bowlPoint;
        cat.bedPoint = bed ? bed : cat.bedPoint;
        cat.toiletPoint = toiletGPS ? toiletGPS : cat.toiletPoint;

        cat.roamingRoot = roamingRoot ? roamingRoot : cat.roamingRoot;

        // 더 이상 할 일 없음 — 충돌 방지 위해 자신 비활성화
        enabled = false;
    }
}
