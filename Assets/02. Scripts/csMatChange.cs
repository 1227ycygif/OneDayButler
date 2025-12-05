using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class csMatChange : XRBaseInteractable
{
    [Header("전환할 머티리얼 할당 (고양이 성격)")]
    public Material[] matUI;

    private int index = 0;
    public csStatUpdate stat;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().material = matUI[index];
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        index++;
        index %= 3;

        this.gameObject.GetComponent<MeshRenderer>().material = matUI[index];

        stat.SetStat(index);
    }
}