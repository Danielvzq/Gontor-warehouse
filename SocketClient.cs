using UnityEngine;

using NativeWebSocket;

public class SocketClient : MonoBehaviour
{
    WebSocket websocket;

    // Start is called before the first frame update
    async void Start()
    {
        websocket = new WebSocket("ws://localhost:8765"); 

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            SendMessage(1, 10, 10);
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            Debug.Log("OnMessage!");
            if (bytes != null)
            {
                // getting the message as a string
                string message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("OnMessage! " + message);
            }
        };
        await websocket.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
        #endif 
    }

    async void SendMessage(int num_agents, int width, int length)
    {
        // Send a json message of the form
        // { "num_agents": int, "width": int, "length": int }
        if (websocket.State == WebSocketState.Open)
        {
            string message = "{\"num_agents\":" + num_agents + ",\"width\":" + width + ",\"length\":" + length + "}";
            await websocket.SendText(message);
        }
    }

    async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
