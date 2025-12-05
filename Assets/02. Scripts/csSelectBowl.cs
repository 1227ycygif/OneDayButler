using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csSelectBowl : MonoBehaviour, IMenuTarget
{
    public string TargetName => "Bowl";

    public void OnMenuAction(string actionName)
    {
        Debug.Log("인터페이스 메서드 실행: " + actionName);

        switch (actionName)
        {
            case "Fill":
                this.gameObject.GetComponent<csBowl>().Feed();
                Debug.Log("밥그릇 채우기 실행");
                break;
            case "Empty":
                this.gameObject.GetComponent<csBowl>().Clear();
                Debug.Log("밥그릇 비우기 실행");
                break;
        }
    }
}
