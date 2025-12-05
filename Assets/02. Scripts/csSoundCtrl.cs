using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Audio; // AudioMixer 연동

public class csSoundCtrl : XRBaseInteractable
{
    [Header("참조")]
    public Slider slider;                    // Slider
    public RectTransform clampRect;          // ★ 이동을 제한할 기준: BackImg 또는 FillBar(트랙)

    [Header("옵션")]
    public bool horizontal = true;
    public bool clampInsideArea = true;
    public float extraPadding = 0f;          // 가장자리 여유(픽셀/로컬)
    [Tooltip("감도(배율) : 1=원래대로, 2=두 배 빨리, 0.5=절반")]
    [Range(0f, 3f)] public float dragGain = 1f;

    [Header("믹서 연결")]
    public AudioMixer mixer;                      // Mixer 에셋 드래그
    public enum ControlTarget { Master, BGM, SFX }
    [Header("옵션")]
    public ControlTarget controlTarget;
    private string exposedParam;                // 설정한 믹서 파라미터 이름 ( MasterVolume / BGMVolume / SFXVolume )

    [Tooltip("Slider 0~1 값을 dB(로그스케일)로 변환할지 여부")]
    public bool useLogScale = true;

    [Tooltip("dB 범위(보통 -80 ~ 0)")]
    public float minDb = -80f;
    public float maxDb = 0f;

    private csSoundManager manager;

    IXRSelectInteractor draggingInteractor;
    bool dragging;

    protected override void Awake()
    {
        base.Awake();

        if (!slider) slider = GetComponentInParent<Slider>();

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;

        Collider col = GetComponent<Collider>();
        if (col && (colliders == null || !colliders.Contains(col))) colliders.Add(col);

        // exposedParam 설정
        switch (controlTarget)
        {
            case ControlTarget.Master:
                exposedParam = "MasterVolume";
                break;
            case ControlTarget.BGM:
                exposedParam = "BGMVolume";
                break;
            case ControlTarget.SFX:
                exposedParam = "SFXVolume";
                break;
        }
    }

    private void Start()
    {
        manager = FindAnyObjectByType<csSoundManager>();
    }

    void Update()
    {
        if (!dragging || draggingInteractor == null) return;
        if (clampRect == null) return;

        // 1. 현재 ray hitPoint 가져오기
        Vector3 hitPoint = draggingInteractor.GetAttachTransform(this).position;
        // 2. clampRect 기준 local 좌표로 변환
        Vector3 localPoint = clampRect.InverseTransformPoint(hitPoint);

        // 3. 슬라이더 방향축 값 추출 (가로/세로)
        float axisPos = horizontal ? localPoint.x : localPoint.y;
        // 4. clampRect 크기를 가져옴
        float halfSize = (horizontal ? clampRect.rect.width : clampRect.rect.height) * 0.5f;

        // 5. 범위 정규화
        float normalized = Mathf.InverseLerp(-halfSize - extraPadding, halfSize + extraPadding, axisPos);
        // 6. 슬라이더 value 갱신
        float v = Mathf.Clamp01(normalized);
        if (slider) slider.value = v;
        // 7. 슬라이더 값으로 믹서 적용
        SetMixerFromSlider();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        manager.Click();

        draggingInteractor = args.interactorObject;
        dragging = true;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {

        if (args.interactorObject == draggingInteractor)
        {
            draggingInteractor = null; 
            dragging = false;
        }
    }

    void SetMixerFromSlider()
    {
        if (mixer == null || string.IsNullOrEmpty(exposedParam)) return;

        // Slider.value(0~1) → AudioMixer dB로 반영
        float dB;
        if (useLogScale)
        {
            // 0을 로그에 넣지 않도록 아주 작은 값으로 보정
            float v = Mathf.Max(slider.value, 0.0001f);
            dB = Mathf.Log10(v) * 20f;          // 0~1 → dB
        }
        else
        {
            dB = Mathf.Lerp(minDb, maxDb, slider.value); // 선형 매핑
        }

        dB = Mathf.Clamp(dB, minDb, maxDb);
        mixer.SetFloat(exposedParam, dB);

    }
}