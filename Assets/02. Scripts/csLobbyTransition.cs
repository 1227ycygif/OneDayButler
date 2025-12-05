using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class csLobbyTransition : XRBaseInteractable
{
    // 인스펙터에서 할당해 줄 예정
    public Renderer FadeOutTransition;
    public GameObject[] FadeoutObj;

    public float transitionTime;
    // 알파값 증가 적용을 위한 내부 변수
    private float dAlpha;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        StartCoroutine(AlphaTransition());
    }

    IEnumerator AlphaTransition()
    {
        Material matFadeOut = FadeOutTransition.material;       // 머테리얼 인스턴스화

        dAlpha = 0f;

        while (dAlpha < 1f)
        {
            dAlpha += Time.deltaTime / transitionTime;
            Color tempC = matFadeOut.color;
            tempC.a = Mathf.Clamp01(dAlpha);
            matFadeOut.color = tempC;

            yield return null;
        }

        // 색상 제대로 적용 안될수도 있으니
        matFadeOut.color = new Color(0, 0, 0, 1f);

        Debug.Log("씬 전환 준비 완료");

        // 비활성 오브젝트들 비활성
        foreach(GameObject obj in FadeoutObj)
        {
            if (obj.activeSelf == true) obj.SetActive(false);
        }

        SceneManager.LoadScene("scMainRoom");
    }
}
