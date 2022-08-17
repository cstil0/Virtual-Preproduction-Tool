using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeDebugText : MonoBehaviour
{
    public GameObject debugPanelText;

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.name == "ancient_vase" && gameObject.layer == 3) 
        //    debugPanelText.GetComponent<Text>().text = "Collided with ancient vase";
        //else
        //{
        //    debugPanelText.GetComponent<Text>().text = other.gameObject.name;
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
