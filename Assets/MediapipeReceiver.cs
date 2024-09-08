using UnityEngine;
using NativeWebSocket;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MediapipeReceiver : MonoBehaviour
{
    public Transform rightShoulder; 
    public Transform rightElbow;    
    public Transform rightWrist;    

    private WebSocket websocket;

    async void Start()
    {
        // PythonサーバーのURLを指定
        string serverUri = "ws://localhost:8000";  
        
        websocket = new WebSocket(serverUri);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connection established.");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            
            // 受信したデータをコンソールに出力
            Debug.Log($"Received raw data: {message}");

            try
            {
                // JSONデータをリストに変換
                List<Dictionary<string, float>> keypoints = JsonConvert.DeserializeObject<List<Dictionary<string, float>>>(message);

                if (keypoints != null)
                {
                    Debug.Log($"Parsed {keypoints.Count} keypoints."); // データのパース結果をログに出力
                    UpdateAvatarPose(keypoints);
                }
                else
                {
                    Debug.LogWarning("Parsed keypoints are null.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing JSON data: {e.Message}");
            }
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket Error: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket connection closed.");
        };

        try
        {
            await websocket.Connect();
            Debug.Log("Connected to WebSocket server.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during WebSocket connection: {ex.Message}");
        }
    }

    void UpdateAvatarPose(List<Dictionary<string, float>> keypoints)
    {
        if (keypoints == null || keypoints.Count == 0)
        {
            Debug.LogWarning("No keypoints data received or data is empty.");
            return;
        }

        // 右手のキーポイントデータを使用してアバターのボーンを更新
        try
        {
            rightShoulder.position = new Vector3(keypoints[12]["x"], keypoints[12]["y"], keypoints[12]["z"]);
            rightElbow.position = new Vector3(keypoints[14]["x"], keypoints[14]["y"], keypoints[14]["z"]);
            rightWrist.position = new Vector3(keypoints[16]["x"], keypoints[16]["y"], keypoints[16]["z"]);
            Debug.Log("Updated avatar pose with new keypoints.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating avatar pose: {e.Message}");
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    private void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
        #endif
    }
}
