using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;

# if UNITY_EDITOR
using UnityEditor;
# endif

[CreateAssetMenu(menuName="Switch/Switch", fileName="Switch_")]
public class Switch : ScriptableObject
{
    #region Event
    public delegate void DataUploadedHandler(Switch switchObj);
    #endregion

    [Header("Text")]
    [SerializeField]
    private string id;
    [SerializeField]
    private string description;
    [SerializeField]
    private string messageID;

    [Header("Setting")]
    [SerializeField]
    private string ip;
    [SerializeField]
    private string netgearEmail;
    [SerializeField]
    private string netgearPW;

    [Header("Port")]
    [SerializeField]
    private List<Port> ports = new List<Port>();

    [Header("Option")]
    [SerializeField]
    private bool isNotUse;

    public string ID => id;
    public string Description => description;
    public string MessageID => messageID;
    public string IP => ip;
    public string NetgearEmail => netgearEmail;
    public string NetgearPW => netgearPW;
    public List<Port> Ports => ports;
    public bool IsNotUse => isNotUse;

    public event DataUploadedHandler onDataUploaded;

    void CreatePort(string portName, string speed)
    {
        ports.Add(new Port(portName, speed)); // Add List
    }

    public void ResponseSwitchData(string portName, string speed)
    {
        var port = ports.FirstOrDefault(p => p.ID == portName); // 포트 탐색

        if (port is null)
            CreatePort(portName, speed); // 없는 포트일 경우 - 생성
        else
            port.Speed = speed; // 있는 포트일 경우 - 수정
    }

    public string ToJson(JArray[] workData)  // POST body setting
    {
        var jsonObject = new JObject(                                //  -----JSON data format-----
            new JProperty("message_id", messageID),          // "message_id": 169164026,
            new JProperty("switchIP", IP),                            // "switchIP": "192.168.20.72",
            new JProperty("netgearemail", NetgearEmail),     // "netgearemail": "cot.remote@gmail.com",
            new JProperty("netgearPW", NetgearPW),          // "netgearPW": "keti123Q!",
            new JProperty("work", workData)                      // "work":[["lookup"]], [['change', ['g1'], 10]] => lookup: 데이터 조회, change: speed 변경
        );

        var json = jsonObject.ToString();
        return json;
    }
    public void ModifyPortSpeed(List<string> portNamesToUpdate, string newSpeed)
    {
        List<string> portsToUpdate = ports.Where(port => portNamesToUpdate.Contains(port.ID) && port.Speed != newSpeed)
                                                          .Select(port => port.ID)            
                                                          .ToList();

        SwitchManager.Instance.UpdatePortSpeed(this, portsToUpdate, newSpeed);
    }

    #region Callback
    public void OnDataUploaded()
    {
        onDataUploaded?.Invoke(this);
    }
    #endregion
}
