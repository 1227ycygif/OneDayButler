using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csSelectToilet : MonoBehaviour, IMenuTarget
{
    public string TargetName => "Toilet";

    public void OnMenuAction(string actionName)
    {
        switch (actionName)
        {
            case "Clear":
                gameObject.GetComponent<csToilet>().Cleanup();
                Debug.Log("화장실 청소 실행");
                break;
        }
    }
}
