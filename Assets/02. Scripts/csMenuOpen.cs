using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class csMenuOpen : XRBaseInteractable
{
    [Space(10)]
    [Header("오브젝트 메뉴창 연결")]
    [Tooltip("선택으로 활성화할 메뉴 오브젝트 넣기")]
    [SerializeField]
    private GameObject dropMenu;
    private csSoundManager mixer;

    private bool isMenuOpened = false;

    private void Start()
    {
        dropMenu.SetActive(isMenuOpened);
        mixer = FindAnyObjectByType<csSoundManager>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        mixer.Click();

        if(dropMenu == null)
        {
            Debug.LogWarning("error: 메뉴 오브젝트 미지정.");
            return;
        }

        isMenuOpened = !isMenuOpened;
        dropMenu.SetActive(isMenuOpened);
        Canvas.ForceUpdateCanvases();

        Debug.Log($"메뉴창 {(isMenuOpened ? "열림" : "닫힘")}");
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        Debug.Log("grab 버튼 뗌");
    }
}
