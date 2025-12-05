using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class csLobbyButton : XRBaseInteractable
{
    [Space(10)]
    [Header("타겟 메뉴창 연결")]
    [Tooltip("선택으로 활성화할 메뉴 오브젝트 넣기")]
    [SerializeField]
    private GameObject targetMenu;
    // 3. 초기 실행 시 타겟 메뉴창 비활성화 적용 여부를 판단할 bool 변수
    [Tooltip("타겟 메뉴창을 실행 시에 끌거면 체크")]
    public bool deactiveAtFirst = false; 
    private bool isMenuOpened = false;

    private csSoundManager mixer;

    private void Start()
    {
        // true : 실행 시 타겟 메뉴창을 꺼주세요  /  false : 켠채로 둘래요
        if(deactiveAtFirst)
            targetMenu.SetActive(isMenuOpened);

        mixer = FindAnyObjectByType<csSoundManager>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        mixer.Click();

        if (targetMenu == null)
        {
            Debug.LogWarning("error: 메뉴 오브젝트 미지정.");
            return;
        }

        // 타겟 메뉴창 활성화, 메뉴 전환
        targetMenu.SetActive(true); 
        isMenuOpened = true;

        // 자신이 속한 버튼 묶음 비활성화
        transform.parent.gameObject.SetActive(false);
    }

}
