using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class DontDestroy : MonoBehaviour
{
    [SerializeField]
    private XROrigin[] camera;

    private void Awake()
    {
        camera = FindObjectsOfType<XROrigin>();
    }
    private void Start()
    {
        if(camera.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
