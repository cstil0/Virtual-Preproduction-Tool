using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// this script is used to handle the initial menu page and show the corresponding color buttons according to their selected state
public class InitialMenu : MonoBehaviour
{
    [SerializeField] Color selectedColor;
    [SerializeField] Color normalColor;

    public ModesManager modesManager;

    // set Mixed Reality mode and change the MR and VR button colors to show the selected mode
    public void setMRMode()
    {
        modesManager.mode = ModesManager.eModeType.MIXEDREALITY;

        GameObject MRButton = GameObject.Find("MR Button");
        ColorBlock buttonColors = MRButton.GetComponent<Button>().colors;
        buttonColors.normalColor = selectedColor;
        MRButton.GetComponent<Button>().colors = buttonColors;

        GameObject VRButton = GameObject.Find("VR Button");
        buttonColors = VRButton.GetComponent<Button>().colors;
        buttonColors.normalColor = normalColor;
        VRButton.GetComponent<Button>().colors = buttonColors;
    }

    // set Virtual Reality mode and change the vR and MR button colors to show the selected mode
    public void setVRMode()
    {
        modesManager.mode = ModesManager.eModeType.VIRTUALREALITY;

        GameObject VRButton = GameObject.Find("VR Button");
        ColorBlock buttonColors = VRButton.GetComponent<Button>().colors;
        buttonColors.normalColor = selectedColor;
        VRButton.GetComponent<Button>().colors = buttonColors;

        GameObject MRButton = GameObject.Find("MR Button");
        buttonColors = MRButton.GetComponent<Button>().colors;
        buttonColors.normalColor = normalColor;
        MRButton.GetComponent<Button>().colors = buttonColors;
    }

    public void startTool(){
        modesManager.loadMainScene();
    }

    void Start()
    {

    }

    void Update()
    {
        
    }
}
