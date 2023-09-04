using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchDataPanel : MonoBehaviour
{
    [SerializeField]
    private string id;
    [SerializeField]
    private Switch switchObj;

    public Transform listContainer;
    public List<Button> portNameTxt;
    public List<TMP_Text> speedTxt;
    public TMP_InputField changeSpeedTxt;
    public TMP_Dropdown changeSpeed;

    public Transform changeListContainer;
    public GameObject listPref;
    public List<string> convertPortList;
    
    void Start()
    {
        InitializePortUIElements();
        switchObj.onDataUploaded += UpdateUIOnData;
    }

    void InitializePortUIElements()
    {
        foreach (Transform child in listContainer) // Text array save 
        {
            Button portNameText = child.GetChild(0)?.GetComponent<Button>();
            TMP_Text speedText = child.GetChild(1)?.GetComponent<TMP_Text>();
            
            if (portNameText != null && speedText != null)
            {
                portNameTxt.Add(portNameText);
                speedTxt.Add(speedText);
            }
        }
    }

    void UpdateUIOnData(Switch updatedSwitchObj) // UI update
    {
        if (updatedSwitchObj != switchObj)
            return;

        for (int i = 0; i < switchObj.Ports.Count; i++)
        {
            portNameTxt[i].GetComponentInChildren<TMP_Text>().text = switchObj.Ports[i].ID;
            speedTxt[i].text = switchObj.Ports[i].Speed;
        }
    }

    public void ChangeSpeedData()
    {
        string newSpeed = changeSpeed.options[changeSpeed.value].text;

        Debug.Assert(!string.IsNullOrEmpty(newSpeed), "Text is empty");
        Debug.Assert(convertPortList.Count > 0, "convertPortList is empty.");

        switchObj.ModifyPortSpeed(convertPortList, newSpeed);
        convertPortList.Clear();

        foreach (Transform child in changeListContainer)
        {
            if(child != changeListContainer.transform)
                Destroy(child.gameObject);
        }
    }
    public void ClickPort()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;

        if (clickedButton != null)
        {
            TMP_Text buttonTextComponent = clickedButton.GetComponentInChildren<TMP_Text>();

            if (buttonTextComponent != null)
            {
                string buttonText = buttonTextComponent.text;
                GameObject list = Instantiate(listPref, changeListContainer);
                TMP_Text textComponent = list.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                    textComponent.text = buttonText;

                convertPortList.Add(buttonText);
            }
        }
    }
}
