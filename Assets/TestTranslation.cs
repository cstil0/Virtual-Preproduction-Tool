using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTranslation : MonoBehaviour
{
    public GameObject vase;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.Translate(new Vector3(1.0f, 0.0f, 0.0f), vase.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
