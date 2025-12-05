using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csToilet : MonoBehaviour
{
    [SerializeField]
    // 화장실 청소 여부, 기본 청소된 상태
    private bool isEmpty = true;

    private GameObject ddong;

    private void Awake()
    {
        ddong = GameObject.Find("poop");
    }

    private void Update()
    {
        ddong.SetActive(!isEmpty);
    }

    // 고양이 행동 로직, 출발 전 조회 (액세스 함수)
    public bool CheckEmpty()
    {
        return isEmpty;
    }
    // 고양이 행동 로직, 대변 보기
    public void Poop()
    {
        // 안 비어있을 때
        if (!isEmpty) { return; }
        // 비어있을 때
        else if (isEmpty) { isEmpty = false; }
    }

    public void Cleanup()
    {
        if (isEmpty)
        {
            Debug.Log("이미 청소되어있습니다.");
        }
        else
        {
            isEmpty = true;    // 청소 완료 
        }
    }
}
