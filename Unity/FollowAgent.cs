using UnityEngine;
using System.Collections.Generic;

public class FollowAgent : MonoBehaviour
{
    public int agentID = -1; // ID del agente a seguir
    private Transform target;
    private Vector3 offset = new Vector3(0, 1, -3); // Cámara detrás del agente

    private bool targetFound = false;
    private bool agentNotFoundLogged = false; // Nueva bandera para controlar el log

    void LateUpdate()
    {
        if (target != null)
        {
            // Sigue la posición del agente
            transform.position = target.position + target.rotation * offset;
            
            // Asegura que la cámara herede la rotación del agente
            transform.rotation = target.rotation;
        }
        else if (!targetFound)
        {
            FindTarget(SocketClient.Instance.GetAgents());
        }
    }

    public void SetFollowTarget(Transform newTarget)
    {
        target = newTarget;
        targetFound = true;
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void FindTarget(Dictionary<int, GameObject> agents)
    {
        // Verificar si el agente con el ID está disponible
        if (agents.ContainsKey(agentID))
        {
            SetFollowTarget(agents[agentID].transform);
            Debug.Log("Cámara ahora sigue al agente con ID: " + agentID);
            agentNotFoundLogged = false; // Reiniciar bandera cuando el agente es encontrado
        }
        else if (!agentNotFoundLogged)
        {
            Debug.LogWarning("No se encontró el agente con ID: " + agentID);
            agentNotFoundLogged = true; // Evitar más logs hasta que el agente sea encontrado
        }
    }
}
