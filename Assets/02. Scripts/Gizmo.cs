using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmo : MonoBehaviour
{
    // 기즈모 색상 
    public Color Mycolor = Color.yellow;
    // 기즈모 반지름 
    public float Radius = 8.0f;

    // 폭발 반경을 그려서 보여준다
    private void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;

        Gizmos.color = Mycolor;

        Gizmos.DrawSphere(p, Radius);
    }

}