//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.XR.Interaction.Toolkit;
//using UnityEngine.Audio;

//[RequireComponent(typeof(Collider))]
//[RequireComponent(typeof(Rigidbody))]
//public class csSoundMenuToggle : XRBaseInteractable
//{
//    [Header("필수 참조")]
//    public Toggle targetToggle;

//    [Header("Audio Mixer")]
//    public AudioMixer audioMixer;
//    [Tooltip("Master 그룹 파라미터 이름")]
//    private string masterParam = "MasterVolume";
//    [Tooltip("BGM 그룹 파라미터 이름")]
//    private string bgmParam = "BGMVolume";
//    [Tooltip("SFX 그룹 파라미터 이름")]
//    private string sfxParam = "SFXVolume";

//    public enum ControlTarget { Master, BGM, SFX }
//    [Header("옵션")]
//    public ControlTarget controlTarget;

//    protected override void Awake()
//    {
//        base.Awake();
//        if (!targetToggle) targetToggle = GetComponent<Toggle>();

//        // XR용 Collider/Rigidbody 세팅
//        Rigidbody rb = GetComponent<Rigidbody>();
//        rb.isKinematic = true; rb.useGravity = false;

//        Collider col = GetComponent<Collider>();
//        if (colliders != null && col && !colliders.Contains(col))
//            colliders.Add(col);
//    }

//    protected override void OnSelectEntered(SelectEnterEventArgs args)
//    {
//        base.OnSelectEntered(args);

//        if (!targetToggle || audioMixer == null) return;

//        // Toggle 상태 반전
//        targetToggle.isOn = !targetToggle.isOn;

//        // 상태 적용
//        ApplyToMixer(targetToggle.isOn);
//    }

    //void ApplyToMixer(bool on)
    //{
    //    string param = "";
    //    switch (controlTarget)
    //    {
    //        case ControlTarget.Master:
    //            param = masterParam;
    //            break;
    //        case ControlTarget.BGM:
    //            param = bgmParam;
    //            break;
    //        case ControlTarget.SFX:
    //            param = sfxParam;
    //            break;
    //    }

    //    if (string.IsNullOrEmpty(param)) return;

    //    // 켜기 → 0dB, 끄기 → -80dB (사실상 음소거)
    //    audioMixer.SetFloat(param, on ? 0f : -80f);

    //    Debug.Log($"{controlTarget} {(on ? "ON" : "OFF")}");
    //}
//}

//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Audio;
//using UnityEngine.EventSystems;          // IPointer*
//using UnityEngine.InputSystem;          // InputActionReference

//// XRBaseInteractable 제거! => UI 레이캐스터가 처리, 물리 충돌 없음
//public class csSoundMenuToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
//{
//    [Header("필수 참조")]
//    public Toggle targetToggle;          // 동일 오브젝트의 Toggle
//    public AudioMixer audioMixer;

//    public enum ControlTarget { Master, BGM, SFX }
//    public ControlTarget controlTarget = ControlTarget.Master;

//    [Header("입력 (Grab 버튼)")]
//    [Tooltip("보통 XRI RightHand/Select 또는 LeftHand/Select")]
//    public InputActionReference grabActionRight;   // 선택
//    public InputActionReference grabActionLeft;    // 선택(옵션)

//    [Header("파라미터 이름")]
//    public string masterParam = "MasterVolume";
//    public string bgmParam = "BGMVolume";
//    public string sfxParam = "SFXVolume";

//    bool hovered;                                   // UI 위에 레이가 올라왔는가

//    void Reset()
//    {
//        targetToggle = GetComponent<Toggle>();
//    }

//    void OnEnable()
//    {
//        if (!targetToggle) targetToggle = GetComponent<Toggle>();

//        if (grabActionRight) { grabActionRight.action.performed += OnGrab; grabActionRight.action.Enable(); }
//        if (grabActionLeft) { grabActionLeft.action.performed += OnGrab; grabActionLeft.action.Enable(); }
//    }

//    void OnDisable()
//    {
//        if (grabActionRight) { grabActionRight.action.performed -= OnGrab; grabActionRight.action.Disable(); }
//        if (grabActionLeft) { grabActionLeft.action.performed -= OnGrab; grabActionLeft.action.Disable(); }
//    }

//    // === UI 포인터 이벤트 ===
//    public void OnPointerEnter(PointerEventData e) { hovered = true; }
//    public void OnPointerExit(PointerEventData e) { hovered = false; }

//    // 클릭(트리거/버튼으로 UI Press를 쏘는 경우)도 지원
//    public void OnPointerClick(PointerEventData e)
//    {
//        ToggleNow();
//    }

//    // Grab(Grip) 눌렀을 때: 현재 UI가 hover 중이면 토글
//    void OnGrab(InputAction.CallbackContext ctx)
//    {
//        if (!hovered) return;
//        ToggleNow();
//    }

//    void ToggleNow()
//    {
//        if (!targetToggle || !audioMixer) return;

//        targetToggle.isOn = !targetToggle.isOn;
//        ApplyToMixer(targetToggle.isOn);
//    }

//    void ApplyToMixer(bool on)
//    {
//        string param = null;
//        switch (controlTarget)
//        {
//            case ControlTarget.Master:
//                param = "MasterVolume";
//                break;
//            case ControlTarget.BGM:
//                param = "BGMVolume";
//                break;
//            case ControlTarget.SFX:
//                param = "SFXVolume";
//                break;
//        }
//        if (string.IsNullOrEmpty(param)) return;

//        // On = 0dB, Off = -80dB
//        audioMixer.SetFloat(param, on ? 0f : -80f);
//        // Debug.Log($"{controlTarget} {(on ? "ON" : "OFF")}");
//    }
//}

