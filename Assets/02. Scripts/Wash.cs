using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wash : MonoBehaviour
{
    public csSelectCat cat;             // 플레이어
    public string[] showerToolTags;            // 순서대로 필요한 태그
    public GameObject[] showerToolsPrefabs;
    public Transform[] showerToolsGPS;
    public ParticleSystem ShampooParticles;   //실행할 파티클

    int step = 0;

    private void Start()
    {
        ShampooParticles.Stop();
        Debug.Log("파티클 꺼짐");
    }

    public void WashStart()
    { 
        for (int i = 0; i < showerToolsPrefabs.Length; i++)
        {
            Instantiate(showerToolsPrefabs[i]);
            showerToolsPrefabs[i].transform.position = showerToolsGPS[i].position;
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("tq");
        if (step >= showerToolTags.Length) return;

        if (other.CompareTag(showerToolTags[step]))
        {
            Destroy(other.gameObject);

            step++;
            Debug.Log("씻는중 : " + step);

            if (step >= showerToolTags.Length)
            {
                Debug.Log("씻기기 완료");
                step = 0;
                if (cat) cat.OnMenuAction("EndWash");

            }
        }

         if (other.CompareTag(showerToolTags[0]))       {ShampooParticles.Play(); Debug.Log("샴푸 들어옴");   }
         else if (other.CompareTag(showerToolTags[1]))  {ShampooParticles.Stop(); Debug.Log("샤워기 들어옴");  }
    }
}
