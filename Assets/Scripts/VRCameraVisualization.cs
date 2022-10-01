using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VRCameraVisualization : MonoBehaviour
{
    bool keyDown;
    // LO GUAI SERIA LLEGIR DINAMICAMENT LES TEXTURES QUE HI HA
    public RenderTexture[] cameraTextures;
    // used to know which camera texture has to be shown
    int cameraCount;

    // Start is called before the first frame update
    void Start()
    {
        GameObject cameraPanel = gameObject.transform.GetChild(0).gameObject;
        keyDown = false;
        cameraPanel.SetActive(false);
        cameraCount = -1;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject cameraPanel = gameObject.transform.GetChild(0).gameObject;
        OVRInput.Update();
        if (OVRInput.Get(OVRInput.Button.One))
        {
            // do it only if it is the first time it is pressed
            if (!keyDown)
            {
                if (cameraCount == -1)
                    cameraPanel.SetActive(!cameraPanel.activeSelf);
                else
                {
                    Material cameraMat = cameraPanel.GetComponent<Renderer>().material;
                    cameraMat.mainTexture = cameraTextures[cameraCount];
                }
                keyDown = true;
                cameraCount += 1;
                cameraCount = cameraCount >= cameraTextures.Length ? cameraCount : -1;
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
