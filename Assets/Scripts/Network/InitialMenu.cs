using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using Microsoft.MixedReality.Toolkit;

public class InitialMenu : MonoBehaviour
{
    public ModesManager modesManager;

    public void setDirectorRole() {
        modesManager.role = ModesManager.eRoleType.DIRECTOR;
    }

    public void setAssitantRole()
    {
        modesManager.role = ModesManager.eRoleType.ASSISTANT;
    }

    public void setMRMode()
    {
        modesManager.mode = ModesManager.eModeType.MIXEDREALITY;
    }

    public void setVRMode()
    {
        modesManager.mode = ModesManager.eModeType.VIRTUALREALITY;
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
