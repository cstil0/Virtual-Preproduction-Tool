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
    GameObject cameraPanel;

    // Start is called before the first frame update
    void Start()
    {
        cameraPanel = gameObject.transform.GetChild(0).gameObject;
        keyDown = false;
        cameraPanel.SetActive(false);
        cameraCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
        if (OVRInput.Get(OVRInput.Button.One))
        {
            // do it only if it is the first time it is pressed
            if (!keyDown)
            {
                Material cameraMat = cameraPanel.transform.GetComponent<UnityEngine.UI.Image>().material;
                // take the absolute value so that when cameraCount = -1 we do not get an error
                cameraMat.mainTexture = cameraTextures[Mathf.Abs(cameraCount)];

                if (cameraCount <= 0)
                    cameraPanel.SetActive(!cameraPanel.activeSelf);

                keyDown = true;
                cameraCount += 1;
                // if we passed the limit of cameras return to -1 so that we hide the panel and then show it again when pressing the button
                cameraCount = cameraCount >= cameraTextures.Length ? -1 : cameraCount;
            }
        }
        // once it is up again, set it to false
        else
        {
            keyDown = false;
        }
    }
}
