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
    private Coordenadas puntosRecogida;
    private Dictionary<int, Queue<Vector3>> agentPaths = new Dictionary<int, Queue<Vector3>>();
    private Dictionary<int, Queue<Vector3>> agentMetas = new Dictionary<int, Queue<Vector3>>();
    private Dictionary<int, GameObject> agents = new Dictionary<int, GameObject>();
    private Dictionary<int, bool> agentPackageStatus = new Dictionary<int, bool>(); // Diccionario para almacenar el estado de tiene_paquete de cada agente
    [SerializeField] private GameObject agentPrefab; // Prefab del agente
    [SerializeField] private GameObject packagePrefab;  // Prefab del paquete

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

    public Dictionary<int, GameObject> GetAgents()
    {
        return agents;
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

        if (data.ContainsKey("punto_recogida"))
        {
            // Convertir el punto de recogida de un array de enteros a Coordenadas
            List<int> punto = JsonConvert.DeserializeObject<List<int>>(data["punto_recogida"].ToString());
            if (punto.Count == 2)
            {
                puntosRecogida = new Coordenadas(punto[0], punto[1]);
                Debug.Log("Punto de recogida actualizado: " + puntosRecogida);
            }
            else
            {
                Debug.LogError("El punto de recogida no tiene el formato esperado.");
            }
        }

        // Continuar con el procesamiento de las rutas de los agentes
        if (data.ContainsKey("rutas"))
        {
            List<List<int[]>> rutas = JsonConvert.DeserializeObject<List<List<int[]>>>(data["rutas"].ToString());
            List<List<int[]>> metas = JsonConvert.DeserializeObject<List<List<int[]>>>(data["metas"].ToString());
            Debug.Log("Recibidas rutas para " + rutas.Count + " agentes.");

            for (int i = 0; i < rutas.Count; i++)
            {
                if (!agents.ContainsKey(i))
                {
                    // Crear un nuevo agente en la posición inicial
                    GameObject newAgent = Instantiate(agentPrefab);
                    agents[i] = newAgent;
                    Debug.Log("Nuevo agente creado en Unity con ID: " + i);
                }

                Queue<Vector3> pathQueue = new Queue<Vector3>();

                foreach (var step in rutas[i])
                {
                    Vector3 worldPosition = GridManager.Instance.GetWorldPosition(new Coordenadas(step[0], step[1]));
                    pathQueue.Enqueue(worldPosition);
                }

                Queue<Vector3> metaQueue = new Queue<Vector3>();

                foreach (var meta in metas[i])
                {
                    Vector3 worldPosition = GridManager.Instance.GetWorldPosition(new Coordenadas(meta[0], meta[1]));
                    metaQueue.Enqueue(worldPosition);
                }

                agentPaths[i] = pathQueue;
                agentMetas[i] = metaQueue;

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
    float rotationSpeed = 5f; // Ajusta este valor para controlar la velocidad de rotación

    foreach (var agent in agents)
    {
        if (agentPaths.ContainsKey(agent.Key) && agentPaths[agent.Key].Count > 0)
        {
            Vector3 currentPosition = agent.Value.transform.position;
            Vector3 nextPosition = agentPaths[agent.Key].Peek();

            // Calcular la dirección del movimiento
            Vector3 direction = nextPosition - currentPosition;

            // Rotar el agente lentamente hacia la dirección del movimiento
            if (direction != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
                agent.Value.transform.rotation = Quaternion.Slerp(agent.Value.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Mover el agente
            agent.Value.transform.position = Vector3.MoveTowards(currentPosition, nextPosition, Time.deltaTime * 5f);

            // Si ha llegado a la siguiente posición, la eliminamos de la cola
            if (Vector3.Distance(currentPosition, nextPosition) < 0.10f)
            {
                agentPaths[agent.Key].Dequeue();

                Vector3 currentMeta = GetNextMetaForAgent(agent.Key);

                if (Vector3.Distance(currentPosition, currentMeta) < 0.1f)
                {
                    Debug.Log("El agente ha llegado a su meta.");
                    var meta = agentMetas[agent.Key].Dequeue();
                    ActualizaPaquete actualiza = agent.Value.GetComponent<ActualizaPaquete>();
                    if (actualiza != null)
                    {
                        Vector3 worldPosition = GridManager.Instance.GetWorldPosition(new Coordenadas(0,0));
                        bool isPackagePickedUp = Vector3.Distance(meta,worldPosition) < 0.1f;
                        actualiza.ActualizaPaqueteMethod();
                    }
                }
            }
        }
    }
}

private Vector3 GetNextMetaForAgent(int agentKey)
{
    // Obtén la meta actual del agente de la lista de metas
    if (agentPaths.ContainsKey(agentKey) && agentPaths[agentKey].Count > 0)
    {
        return agentMetas[agentKey].Peek();  // Devolver la siguiente meta del agente
    }
    return Vector3.zero;  // Si no hay más metas, devolver un vector nulo
}


    public Coordenadas GetTileCoordinatesFromWorldPosition(Vector3 worldPosition)
    {
        // Convertir la posición mundial al sistema de coordenadas del grid
        int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / GridManager.Instance.TileSize);
        int y = Mathf.FloorToInt((worldPosition.z - transform.position.z) / GridManager.Instance.TileSize);

        return new Coordenadas(x, y);
    }
    
    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}