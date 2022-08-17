using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugPanel : MonoBehaviour
{
    bool keyDown;
    public GameObject canvas;

    // Start is called before the first frame update
    void Start()
    {
        GameObject panel = canvas.transform.GetChild(0).gameObject;
        keyDown = false;
        panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject panel = canvas.transform.GetChild(0).gameObject;
        OVRInput.Update();
        if (OVRInput.Get(OVRInput.Button.One))
        {
            // do it only if it is the first time it is pressed
            if (!keyDown)
            {
                keyDown = true;
                panel.SetActive(!panel.activeSelf);
            }
            //if (OVRInput.GetDown(OVRInput.Button.One)) { 
            //if (Input.GetKeyDown(KeyCode.M)) || Input.GetJoystickNames(Button.Three))
            //{
        }
        // once it is up again, set it to false
        else
        {
            keyDown = false;
        }
    }
}
