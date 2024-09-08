using UnityEngine;
using NativeWebSocket;
using System.Threading.Tasks;  // Taskを使用するために追加

public class WebSocketTest : MonoBehaviour
{
    private WebSocket websocket;

    async void Start()
    {
        // Pythonサーバーが実行されているPCのIPアドレスまたはホスト名を指定
        string serverUri = "ws://localhost:8765";  // 例: "ws://192.168.1.5:8765"
        
        websocket = new WebSocket(serverUri);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connection Established.");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log($"Received: {message}");
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

            // 接続後にメッセージを送信
            await websocket.SendText("Hello from Unity!");

            // 5000ミリ秒（5秒）待機
            await Task.Delay(5000);

            await websocket.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception during WebSocket connection: {ex.Message}");
        }
    }
}
