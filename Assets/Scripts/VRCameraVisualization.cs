using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script is not used anymore, since previously there was just one canvas in VR to previsualize the view of all cameras by iterating the texture
public class VRCameraVisualization : MonoBehaviour
{
    bool keyDown;
    public RenderTexture[] cameraTextures;
    // used to know which camera texture has to be shown
    int cameraCount;
    GameObject cameraPanel;

    void Start()
    {
        cameraPanel = gameObject.transform.GetChild(0).gameObject;
        keyDown = false;
        cameraPanel.SetActive(false);
        cameraCount = 0;
    }

    void Update()
    {
        OVRInput.Update();
        if (OVRInput.Get(OVRInput.Button.One))
        {
            // perform action only if it is the first time the button is pressed
            if (!keyDown)
            {
                Material cameraMat = cameraPanel.transform.GetComponent<UnityEngine.UI.Image>().material;
                // take the absolute value so that when cameraCount = -1 we do not get an error
                cameraMat.mainTexture = cameraTextures[Mathf.Abs(cameraCount)];

                if (cameraCount <= 0)
                    cameraPanel.SetActive(!cameraPanel.activeSelf);

                keyDown = true;
                cameraCount += 1;
                // if we passed the limit of cameras return count to -1 so that the panel is hidden and then show it again when pressing the button
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
