using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas

public class SceneController : MonoBehaviour
{
    // Referencias a las cámaras
    public Camera mainCamera;  // Cámara principal (en la escena principal)
    public Camera creditCamera; // Cámara de los créditos (en la escena principal)

    // Método para cargar la escena de créditos de manera aditiva
    public void CargarCreditos()
    {
        // Desactivar todos los Canvas en la escena principal
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();  // Obtener todos los Canvas
        foreach (Canvas canvas in allCanvases)
        {
            canvas.gameObject.SetActive(false); // Desactiva cada Canvas encontrado
        }

        // Desactivar la cámara principal
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false); // Desactiva la cámara principal
        }

        // Activar la cámara de los créditos
        if (creditCamera != null)
        {
            creditCamera.gameObject.SetActive(true); // Activa la cámara de los créditos
        }

        // Cargar la escena de créditos de manera aditiva
        SceneManager.LoadScene("Creditos", LoadSceneMode.Additive);
    }

    // Método para cerrar la escena de créditos y restaurar la cámara principal
    public void CerrarCreditos()
    {
        // Descargar la escena de créditos
        SceneManager.UnloadSceneAsync("Creditos");

        // Restaurar la cámara principal
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true); // Reactiva la cámara principal
        }

        // Desactivar la cámara de los créditos
        if (creditCamera != null)
        {
            creditCamera.gameObject.SetActive(false); // Desactiva la cámara de los créditos
        }

        // Activar nuevamente todos los Canvas en la escena principal
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();  // Obtener todos los Canvas
        foreach (Canvas canvas in allCanvases)
        {
            canvas.gameObject.SetActive(true); // Activa cada Canvas encontrado
        }
    }
}