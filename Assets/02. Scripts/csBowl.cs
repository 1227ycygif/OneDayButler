using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csBowl : MonoBehaviour
{
    [SerializeField]
    // 밥그릇 사료 여부, 기본 비어있는 상태
    private bool isEmpty = true;
    [SerializeField]
    private GameObject food;

    private void Awake()
    {
        food = GameObject.Find("food");
    }

    private void Update()
    {
        food.SetActive(!isEmpty);
    }

    // 고양이 로직, 밥그릇 상태 조사 (액세스 함수)
    public bool CheckEmpty()
    {
        return isEmpty;
    }

    // 고양이 로직, 밥 먹기 (액세스 함수)
    public void Eat()
    {
        isEmpty = true;
    }

    // 플레이어 동작, 밥 채우기 (액세스 함수)  
    public void Feed()
    {
        // 이미 밥이 차있으면
        if (!isEmpty)
        {
            Debug.Log("이미 밥을 주었습니다.");
            return;
        }
        // 밥이 안차있다면
        else
        {
            isEmpty = false;
        }
    }

    // 플레이어 동작, 밥그릇 비우기 (액세스 함수)
    public void Clear()
    {
        // 밥그릇이 이미 비어있다면
        if (isEmpty)
        {
            Debug.Log("밥그릇은 이미 비어있습니다.");
            return;
        }
        // 밥그릇이 차있다면
        else
        {
            isEmpty = true;
        }
    }
}
