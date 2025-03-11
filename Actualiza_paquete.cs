using UnityEngine;

public class ActualizaPaquete : MonoBehaviour
{
    private GameObject packagePrefab; // El paquete asociado al agente

    void Start()
    {
        // Buscar el objeto "Package" hijo del agente
        packagePrefab = transform.Find("packagePrefab")?.gameObject;

        // Si el paquete existe, hacerlo invisible al inicio
        if (packagePrefab != null)
        {
            packagePrefab.SetActive(false); // Inicia invisible
        }
    }

    // Llamado cuando el agente llega a la meta (para actualizar el estado del paquete)
    public void ActualizaPaqueteMethod()
    {
        if (packagePrefab == null) return;

        // Si el agente tiene el paquete, lo hace visible
        if (!packagePrefab.activeSelf)
        {
            packagePrefab.SetActive(true);
            Debug.Log("📦 El paquete ha sido asignado y es visible.");
        }
        else
        {
            // Si el agente entrega el paquete, lo hace invisible
            packagePrefab.SetActive(false);
            Debug.Log("📦 El paquete ha sido entregado y ya no es visible.");
        }
    }
}
