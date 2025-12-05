using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csSelectCat : MonoBehaviour, IMenuTarget
{
    public string TargetName => "Cat";

    public Transform washPoint;     // 인스펙터에서 고양이 위치 지정
    public Transform endWashPoint;
    
    public Transform washPlayerPoint;
    public Transform endPlayerPoint;

    public CatAnimation cat;    // 인스펙터에 Cat 할당
    public Transform player;

    public void OnMenuAction(string actionName)
    {
        switch (actionName)
        {
            case "CatSelect":
                Debug.Log("고양이 선택");
                cat.CatSelect();
            break;

            case "CatSelectQuit":
                Debug.Log("고양이 선택 탈출");
                cat.CatSelectQuit();
                break;
            case "Pat":
                // 기능 구현
                Debug.Log("쓰다듬기 실행");
                break;

            case "Wash":
                // 고양이 씻기 코루틴 시작
                cat.StartWash(washPoint);
                Debug.Log("씻기기 실행");
                break;

            case "EndWash": // 필요하면 메뉴에 추가
                cat.EndWash(endWashPoint);
                Debug.Log("씻기기 종료");
                break;
            case "Hungry = zero": // 필요하면 메뉴에 추가
                cat.catStatus.hunger -= 1;
                Debug.Log("Hungry = zero");
                break;
            case "Hungry = ten": // 필요하면 메뉴에 추가
                cat.catStatus.hunger += 1;
                Debug.Log("Hungry = ten");
                break;
            case "Sleep = zero": // 필요하면 메뉴에 추가
                cat.catStatus.sleepiness -= 1;
                Debug.Log("Sleep = zero");
                break;
            case "Sleep heal":
                cat.catStatus.sleepiness += 1;
                break;
            case "happy up":
                cat.catStatus.happiness += 1;
                break;
            case "happy down":
                cat.catStatus.happiness -= 1;
                break;

        }
    }

    void Update()
    {
        //=================== 시연용 코드 ===================
        if (Input.GetKey(KeyCode.Q))
            OnMenuAction("Hungry = zero");
        if (Input.GetKey(KeyCode.W))
            OnMenuAction("Hungry = ten");
        if (Input.GetKey(KeyCode.E))
            OnMenuAction("Sleep = zero");
        if(Input.GetKey(KeyCode.R))
            OnMenuAction("Sleep heal");
        if (Input.GetKey(KeyCode.A))
            OnMenuAction("happy up");
        if (Input.GetKey(KeyCode.S))
            OnMenuAction("happy down");
    }
}