using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP;
using BestHTTP.WebSocket;
using BestMQTT;
using BestMQTT.Packets.Builders;
using BestMQTT.Packets;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Linq;
using Newtonsoft.Json.Linq;

//MQTT 라이브러리 사용
public class MQTT : MonoBehaviour
{
    MQTTClient client;

    [SerializeField]
    private SwitchManager switchManager;
    [SerializeField]
    private string state;

    public string State => state;

    void Start()
    {
        Connect();
    }

    public void Connect()
    {
        client = new MQTTClientBuilder()
        #if !UNITY_WEBGL || UNITY_EDITOR
            .WithOptions(new ConnectionOptionsBuilder().WithTCP("203.254.171.125", 1883))
        #else
            .WithOptions(new ConnectionOptionsBuilder().WithWebSocket("203.254.171.125", 9001).WithTLS())
        #endif
        .WithEventHandler(OnConnected)
        .WithEventHandler(OnDisconnected)
        .WithEventHandler(OnStateChanged)
        .WithEventHandler(OnError)
        .CreateClient();
        client.BeginConnect(ConnectPacketBuilderCallback);
    }

    private void OnConnected(MQTTClient client)
    {
        using (new PacketBufferHelper(client))
        {
            client.CreateSubscriptionBuilder("Network")
            .WithMessageCallback(OnMessage)
            .WithAcknowledgementCallback(OnSubscriptionAcknowledged)
            .WithMaximumQoS(QoSLevels.ExactlyOnceDelivery)
            .BeginSubscribe();
        }
    }

    // received Message Format
    // {    
    //     "digital_twin_id": "DT ID",
    //     "sensor_id": "Sensor ID",
    //     "data": "{\"switches\":[{\"port\":\"g1\",\"speed\":\"Auto\"},{\"port\":\"g2\",\"speed\":\"Auto\"} ... {\"port\":\"l8\",\"speed\":\"Auto\"},{\"message_id\":\"GGwang\"},{\"switch_id\":1}]}",
    //     "rowtime": "0000-00-00 00:00:00.000",
    //     "location": "{}"
    // }
    void Subscribe(string msg) 
    {
        // Data Parsing
        JObject jObject = JObject.Parse(msg);
        if (jObject["data"] is JToken dataToken)
        {
            JArray switchInfo = JObject.Parse((string)dataToken)["switches"] as JArray;

            switchManager.UploadData(switchInfo);          
        }
    }
    
    void Unsubscribe()
    {
        // client.CreateUnsubscribePacketBuilder("Network")
        // .WithAcknowledgementCallback((client, topicFilter, reasonCode) => Debug.Log($"Unsubscribe request to topic filter '{topicFilter}' returned with code: {reasonCode}"))
        // .BeginUnsubscribe();
    }

    //Message Receiver 함수
    private void OnMessage(MQTTClient client, SubscriptionTopic topic, string topicName, ApplicationMessage message)
    {
        //메시지 들어오는 주기: 10 ~ 15s
        var payload = Encoding.UTF8.GetString(message.Payload.Data, message.Payload.Offset, message.Payload.Count);
        // Debug.Log($"topic: {topic} Payload: {payload}");
        Subscribe(payload);
    }

    private void OnSubscriptionAcknowledged(MQTTClient client, SubscriptionTopic topic, SubscribeAckReasonCodes reasonCode)
    {
        // if (reasonCode <= SubscribeAckReasonCodes.GrantedQoS2)
        //     Debug.Log($"Successfully subscribed with topic filter '{topic.Filter.OriginalFilter}'. QoS granted by the server: {reasonCode}");
        // else
        //     Debug.Log($"Could not subscribe with topic filter '{topic.Filter.OriginalFilter}'! Error code: {reasonCode}");
    }

    private void OnDestroy()
    {
        client?.CreateDisconnectPacketBuilder()
        .WithReasonCode(DisconnectReasonCodes.NormalDisconnection)
        .WithReasonString("Bye")
        .BeginDisconnect();
    }
    
    private ConnectPacketBuilder ConnectPacketBuilderCallback(MQTTClient client, ConnectPacketBuilder builder)
    {
        return builder;
    }

    private void OnStateChanged(MQTTClient client, ClientStates oldState, ClientStates newState)
    {
        string state = newState.ToString().ToLower();

        switch(state)
        {
            case "disconnected":
                client = null;
                Connect(); // reconnect
                break;
            case "connected":
                switchManager.OnSwitchInfoRequest(); // MQTT 연결 완료 => 서버에 Switch Data Post 요청 
                break;
        }
    }

    private void OnDisconnected(MQTTClient client, DisconnectReasonCodes code, string reason)
    {
        Debug.Log($"OnDisconnected - code: {code}, reason: '{reason}'");
        client.BeginConnect(ConnectPacketBuilderCallback);
    }

    private void OnError(MQTTClient client, string reason)
    {
        Debug.Log($"OnError reason: '{reason}'");
    }
}