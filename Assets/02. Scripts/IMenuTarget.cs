    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public interface IMenuTarget
{
    // 메인 오브젝트 이름  프로퍼티
    string TargetName { get; }
    // 메뉴 선택 시 실행될 액션 메서드
    void OnMenuAction(string actionName);
}
