using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemsDirectorPanelController : MonoBehaviour
{
    public static ItemsDirectorPanelController instance = null;

    [SerializeField] TMP_Text itemName;
    [SerializeField] Button closeButton;
    [SerializeField] Button removeButton;
    [SerializeField] Button speedMinusButton;
    [SerializeField] Button speedPlusButton;
    [SerializeField] TMP_InputField speedInput;
    [SerializeField] Button trashButton;

    [SerializeField] GameObject panelLayout;
    [SerializeField] GameObject itemsOptionsPanel;
    [SerializeField] GameObject itemButtonPrefab;

    private void Awake()
    {
        if (instance)
        {
            if (instance != this)
                Destroy(gameObject);
        }
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onItemsButtonPressed(TMP_Text buttonText)
    {
        itemName.text = buttonText.text;
        if (buttonText.transform.parent.name.Contains("Camera"))
            removeButton.gameObject.SetActive(false);
        else
            removeButton.gameObject.SetActive(true);

        itemsOptionsPanel.SetActive(true);

    }

    public void onCloseButtonPressed()
    {
        gameObject.SetActive(false);
    }

    public void onSpeedPlusPressed()
    {
        speedInput.text = (int.Parse(speedInput.text) + 1).ToString();
        onSpeedChange();
    }

    public void onSpeedMinusPressed()
    {
        speedInput.text = (int.Parse(speedInput.text) - 1).ToString();
        onSpeedChange();
    }

    public void onSpeedChange()
    {

    }

    public void addNewItemButton(string name)
    {
        GameObject newButton = Instantiate(itemButtonPrefab);
        newButton.transform.parent = panelLayout.transform;
        newButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0.0f, 0.0f, 0.0f);
        newButton.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        newButton.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        
        newButton.transform.GetChild(0).GetComponent<TMP_Text>().text = name;
        newButton.name = name + "Button";
        TMP_Text buttonText = newButton.transform.GetChild(0).GetComponent<TMP_Text>();
        newButton.GetComponent<Button>().onClick.AddListener( delegate { onItemsButtonPressed(buttonText); });
    }
}
