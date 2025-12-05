using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class csOptionMenuToggle : MonoBehaviour
{
    [Header("연결")]
    [Tooltip("열고/닫을 OptionMenu 루트 오브젝트")]
    public GameObject optionMenuRoot;

    [Tooltip("메뉴가 열릴 때 켤 XR Ray (좌/우). 비워두면 건너뜀")]
    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;

    [Tooltip("메뉴가 열릴 때 잠깐 꺼둘 로코모션들(Continuous/Teleport 등)")]
    public List<LocomotionProvider> locomotionsToDisable = new List<LocomotionProvider>();

    [Header("입력(Quest/VR 컨트롤러)")]
    [Tooltip("A/X 버튼을 허용")]
    public bool usePrimaryButton = true;     // A(오른손), X(왼손)
    [Tooltip("B/Y 버튼을 허용")]
    public bool useSecondaryButton = true;   // B(오른손), Y(왼손)

    bool isOpen = false;
    float prevTimeScale = 1f;

    InputDevice leftHand, rightHand;
    bool lastAnyDown = false;

    void Start()
    {
        // 초기 상태
        SetMenu(false, immediate: true);
        // 디바이스 캐시
        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        // 디바이스가 유효하지 않으면 재탐색
        if (!leftHand.isValid) leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (!rightHand.isValid) rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool anyPressed = false;

        if (usePrimaryButton)
        {
            if (leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool lPrim) && lPrim) anyPressed = true;   // X
            if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool rPrim) && rPrim) anyPressed = true; // A
        }
        if (useSecondaryButton)
        {
            if (leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool lSec) && lSec) anyPressed = true;   // Y
            if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool rSec) && rSec) anyPressed = true; // B
        }

        // "한 번 눌렀을 때"만 토글 (엣지 감지)
        if (anyPressed && !lastAnyDown)
        {
            SetMenu(!isOpen);
        }
        lastAnyDown = anyPressed;
    }

    void SetMenu(bool open, bool immediate = false)
    {
        isOpen = open;

        if (optionMenuRoot) optionMenuRoot.SetActive(open);

        // 타임스케일 일시정지/복구
        if (open)
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = prevTimeScale;
        }

        // 이동/텔레포트 비활성화(선택)
        foreach (var loco in locomotionsToDisable)
            if (loco) loco.enabled = !open;

        // 캔버스 즉시 갱신(월드 스페이스 UI일 때 종종 필요)
        if (open) Canvas.ForceUpdateCanvases();
    }
}
