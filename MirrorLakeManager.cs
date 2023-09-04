using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

// HTTP Client class
public class MirrorLakeManager : MonoBehaviour
{   
    #region Singleton
    public static MirrorLakeManager Instance;
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

    [SerializeField] SwitchManager switchManager;

    private string ip = "http://000.000.000.000:0000/keti/v1/digital-twins/";
    private string dt_ID = "000000000000000000000000000000000";
    private string sensorKey = "abcdefghijklmnopqrstuvwxyz";

    // HTTP POST 요청 성공 시 서버에 전송
    // 서버는 요청된 작업을 수행 후 클라이언트가 구독한 토픽에 데이터 발행
    // 클라이언트는 해당 토픽을 구독 후 대기 상태 => 발행된 데이터를 수신 (MQTT - OnMessage함수)
    public IEnumerator Post_RequestSwitchData(string json)
    {
        string url = $"{ip}{dt_ID}/sensors/{sensorKey}/data";
        
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, json))
        {
            byte[] bodyData = new UTF8Encoding().GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
                Debug.Log(request.error);

            request.Dispose();
        }
        yield return null;
    }
}