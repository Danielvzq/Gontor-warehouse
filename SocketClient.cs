using UnityEngine;
using NativeWebSocket;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

public class SocketClient : MonoBehaviour
{
    public static SocketClient Instance { get; private set; }

    private WebSocket websocket;
    private Dictionary<int, Queue<Vector3>> agentPaths = new Dictionary<int, Queue<Vector3>>();
    private Dictionary<int, GameObject> agents = new Dictionary<int, GameObject>();

    [SerializeField] private GameObject agentPrefab; // Prefab del agente

    private async void Awake()
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

        DontDestroyOnLoad(gameObject);
        await ConnectWebSocket();
    }

    private async Task ConnectWebSocket()
    {
        websocket = new WebSocket("ws://localhost:8765");

        websocket.OnOpen += () =>
        {
            Debug.Log("Conectado al servidor WebSocket.");
            SendMessage(); // Envía el grid al iniciar
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error en WebSocket: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Desconectado del servidor.");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Mensaje recibido: " + message);
            UpdateAgentPaths(message); // Asigna las rutas a los agentes
        };

        await websocket.Connect();
    }

    private void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
        #endif

        MoveAgents();
    }

    public async void SendMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            Dictionary<string, object> data = GridManager.Instance.GetGridData();
            string message = JsonConvert.SerializeObject(data);

            await websocket.SendText(message);
            Debug.Log("Mensaje enviado al servidor: " + message);
        }
    }

    private void UpdateAgentPaths(string jsonMessage)
    {
        Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonMessage);

        if (data.ContainsKey("rutas"))
        {
            List<List<int[]>> rutas = JsonConvert.DeserializeObject<List<List<int[]>>>(data["rutas"].ToString());
            Debug.Log("Recibidas rutas para " + rutas.Count + " agentes.");

            for (int i = 0; i < rutas.Count; i++)
            {
                if (!agents.ContainsKey(i))
                {
                    // Crear un nuevo agente en la posición inicial
                    GameObject newAgent = Instantiate(agentPrefab);
                    agents[i] = newAgent;
                    Debug.Log("Nuevo agente creado en Unity: " + i);
                }

                Queue<Vector3> pathQueue = new Queue<Vector3>();

                foreach (var step in rutas[i])
                {
                    Vector3 worldPosition = GridManager.Instance.GetWorldPosition(new Coordenadas(step[0], step[1]));
                    pathQueue.Enqueue(worldPosition);
                }

                agentPaths[i] = pathQueue;

                // Colocar al agente en la primera posición
                if (agentPaths[i].Count > 0)
                {
                    agents[i].transform.position = agentPaths[i].Dequeue();

                    // Centrar el agente en la tile usando TileSize de GridManager
                    float tileSize = GridManager.Instance.TileSize;
                    agents[i].transform.position += new Vector3(tileSize / 2f, 0.05f, tileSize / 2f);

                    Debug.Log("Agente " + i + " centrado en: " + agents[i].transform.position);
                }
            }
        }
    }

    private void MoveAgents()
    {
        foreach (var agent in agents)
        {
            if (agentPaths.ContainsKey(agent.Key) && agentPaths[agent.Key].Count > 0)
            {
                Vector3 nextPosition = agentPaths[agent.Key].Peek();
                Debug.Log("Agente " + agent.Key + " moviéndose a: " + nextPosition);
                agent.Value.transform.position = Vector3.MoveTowards(agent.Value.transform.position, nextPosition, Time.deltaTime * 5f);

                if (Vector3.Distance(agent.Value.transform.position, nextPosition) < 0.1f)
                {
                    agentPaths[agent.Key].Dequeue();
                }
            }
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
