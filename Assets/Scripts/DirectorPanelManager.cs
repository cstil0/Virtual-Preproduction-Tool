using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectorPanelManager : MonoBehaviour
{
    public GameObject multiviewPanel;
    public GameObject aerealviewPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void goToAerialView()
    {
        multiviewPanel.SetActive(false);
        aerealviewPanel.SetActive(true);
    }
    public void goToMainPView()
    {
        multiviewPanel.SetActive(true);
        aerealviewPanel.SetActive(false);
    }
}
