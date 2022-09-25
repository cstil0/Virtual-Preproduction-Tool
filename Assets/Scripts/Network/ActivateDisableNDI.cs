using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDisableNDI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
