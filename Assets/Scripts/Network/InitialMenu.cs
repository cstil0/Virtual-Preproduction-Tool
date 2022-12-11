using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.UI;

public class InitialMenu : MonoBehaviour
{
    public ModesManager modesManager;

    //public void setDirectorRole() {
    //    modesManager.role = ModesManager.eRoleType.DIRECTOR;
    //}

    //public void setAssitantRole()
    //{
    //    modesManager.role = ModesManager.eRoleType.ASSISTANT;
    //}

    public void setMRMode()
    {
        modesManager.mode = ModesManager.eModeType.MIXEDREALITY;

        GameObject MRButton = GameObject.Find("MR Button");
        ColorBlock buttonColors = MRButton.GetComponent<Button>().colors;
        buttonColors.normalColor = new Color(0.6588235f, 0.4117647f, 0.7450981f);
        MRButton.GetComponent<Button>().colors = buttonColors;

        GameObject VRButton = GameObject.Find("VR Button");
        buttonColors = VRButton.GetComponent<Button>().colors;
        buttonColors.normalColor = new Color(0.7924528f, 0.7924528f, 0.7924528f);
        VRButton.GetComponent<Button>().colors = buttonColors;
    }

    public void setVRMode()
    {
        modesManager.mode = ModesManager.eModeType.VIRTUALREALITY;

        GameObject VRButton = GameObject.Find("VR Button");
        ColorBlock buttonColors = VRButton.GetComponent<Button>().colors;
        buttonColors.normalColor = new Color(0.6588235f, 0.4117647f, 0.7450981f);
        VRButton.GetComponent<Button>().colors = buttonColors;

        GameObject MRButton = GameObject.Find("MR Button");
        buttonColors = MRButton.GetComponent<Button>().colors;
        buttonColors.normalColor = new Color(0.7924528f, 0.7924528f, 0.7924528f);
        MRButton.GetComponent<Button>().colors = buttonColors;
    }

    public void startTool(){
        modesManager.loadMainScene();
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
