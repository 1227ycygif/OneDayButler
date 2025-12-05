using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class csLobbyToggle : XRBaseInteractable
{
    [Space(10)]
    [Header("타겟 메뉴창 연결")]
    [Tooltip("선택으로 활성화할 메뉴 오브젝트 넣기")]
    [SerializeField]
    private GameObject targetMenu;
    // 초기 실행 시 타겟 메뉴창 비활성화 적용 여부를 판단할 bool 변수
    [Tooltip("타겟 메뉴창을 실행 시에 끌거면 체크")]
    public bool deactiveAtFirst = false;
    private bool isMenuOpened = false;

    private void Start()
    {
        // true : 실행 시 타겟 메뉴창을 꺼주세요  /  false : 켠채로 둘래요
        if (deactiveAtFirst)
            targetMenu.SetActive(isMenuOpened);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        //base.OnSelectEntered(args);

        if (targetMenu == null)
        {
            Debug.LogWarning("error: 메뉴 오브젝트 미지정.");
            return;
        }

        isMenuOpened = !isMenuOpened;           // toggle
        targetMenu.SetActive(isMenuOpened);     // toggle 결과 적용
    }
}
