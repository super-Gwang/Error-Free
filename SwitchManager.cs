using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityDebug = UnityEngine.Debug;
using SystemDebug = System.Diagnostics.Debug;

public class SwitchManager : MonoBehaviour
{
    #region Singleton
    public static SwitchManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    #region Event
    public delegate void SwitchInfoRequestHandler(Switch switchObj);
    public delegate void PortSpeedConvertHandler(Switch switchObj, List<Port> ports, string newSpeed);
    #endregion

    MirrorLakeManager client;
    [SerializeField]
    private SwitchDatabase switchDatabase;
    [SerializeField]
    private int currentIndex;

    public event SwitchInfoRequestHandler onSwitchInfoRequest;
    public event PortSpeedConvertHandler onPortSpeedConvert;

    private int switchDatabaseSize;
    private JArray[] workData = new JArray[1];
    private JArray[] workData_inner = new JArray[1];
    private JArray workData_port = new JArray();

    void Start()
    {
        client = MirrorLakeManager.Instance; // MirrorLake Singleton

        switchDatabase = Resources.Load<SwitchDatabase>("Switch Database");
        switchDatabaseSize = switchDatabase.Switches.Count;

        onSwitchInfoRequest += Request_SwitchInfo; // Add event 
    }
    
    public void UploadData(JArray switchInfo)
    {
        SystemDebug.Assert(switchInfo != null, "Switches data is null");
        Switch switchObj = null;

        foreach (JObject switchData in switchInfo)
        {
            if (switchData.ContainsKey("port") && switchData.ContainsKey("speed"))
            {
                string port = switchData["port"].ToString();
                string speed = switchData["speed"].ToString()

                switchObj.ResponseSwitchData(port, speed);
            }

            if (switchData.ContainsKey("switch_id"))
            {
                int index = (int)switchData["switch_id"];
                switchObj = switchDatabase.Switches[index];
            }
        }

        switchObj.OnDataUploaded(); // switch data upload complete => UI update
        OnSwitchInfoRequest();
    }

    public void Request_SwitchInfo(Switch switchObj)
    {
        workData[0] = workData_inner[0];
        StartCoroutine(client.Post_RequestSwitchData(TransformDataFormat(switchObj, workData));
    }

    private string TransformDataFormat(Switch switchObj, JArray[] workData)
    {
        StringBuilder jsonData = new StringBuilder();
        jsonData.Append("{\"data\": ");
        jsonData.Append(switchObj.ToJson(workData));
        jsonData.Append("}");

        return jsonData.ToString();
    }   

    public void OnSwitchInfoRequest() // Request switch data info event
    {
        workData_inner[0] = new JArray("lookup");
        onSwitchInfoRequest?.Invoke(switchDatabase.Switches[currentIndex]);

        currentIndex = (currentIndex + 1) % switchDatabaseSize; // next switch request
    }

    public void UpdatePortSpeed(Switch switchObj, List<string> portsToUpdate, string newSpeed)
    {
        foreach(string port in portsToUpdate)
        {
            workData_port.Add(port);
        }

        workData_inner[0] = new JArray("change", workData_port, newSpeed);
        Request_SwitchInfo(switchObj);
    }
}
