using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsMenu : MonoBehaviour
{
    public GameObject ItemsMenu_object;
    public GameObject LightingMenu_object;

    // ESTARIA GUAI QUE ES POSI EL QUE HA CLICAT EL BOTÓ, PERÒ DE MOMENT EL FAREM PER DRETANS
    public GameObject RightController;

    public GameObject FocusPrefab;

    // Start is called before the first frame update
    void Start()
    {
        ItemsButton();
    }

    public void ItemsButton()
    {
        // Show Items Menu
        ItemsMenu_object.SetActive(true);
        LightingMenu_object.SetActive(false);
    }

    public void LightingButton()
    {
        // Show Lighting Menu
        ItemsMenu_object.SetActive(false);
        LightingMenu_object.SetActive(true);
    }

    public void FocusButton()
    {
        // instantiate a new focus in the hands position and looking forward
        GameObject focus = Instantiate(FocusPrefab);
        focus.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        focus.transform.Rotate(0.0f, 180.0f, 0.0f);
        focus.transform.position = RightController.transform.position;
    }

    // Update is called once per frame
    //void Update()
    //{

    //}
}
