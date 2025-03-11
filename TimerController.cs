using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro; // Añadido para TextMesh Pro

public class TimerController : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Tiempo inicial en segundos")]
    [SerializeField] private float initialTime = 0f; // Comienza desde 0

    [Tooltip("Velocidad del timer (1 = normal, 2 = doble velocidad, 0.5 = mitad de velocidad)")]
    [Range(0.1f, 10f)]
    [SerializeField] private float timerSpeed = 1f;

    [Tooltip("¿El timer cuenta hacia atrás? Si es falso, cuenta hacia adelante")]
    [SerializeField] private bool countDown = false; // Cambiado a false

    [Tooltip("¿Reiniciar automáticamente cuando llegue a 0? (solo para cuenta regresiva)")]
    [SerializeField] private bool autoReset = false;

    [Header("UI Elements")]
    [Tooltip("Texto UI para mostrar el tiempo (opcional)")]
    [SerializeField] private TextMeshProUGUI timerText; // Cambiado a TextMeshProUGUI

    [Header("Events")]
    [Tooltip("Evento que se dispara cuando el timer llega a 0 (solo para cuenta regresiva)")]
    public UnityEvent onTimerEnd;

    [Tooltip("Evento que se dispara cada segundo")]
    public UnityEvent onSecondTick;

    // Variables privadas
    private float currentTime;
    private bool isRunning = false;
    private float lastSecondTick;

    void Start()
    {
        // Llamamos a ResetTimer para configurar el tiempo inicial a 0
        ResetTimer();

        // Llamamos a StartTimer para que el temporizador comience automáticamente
        StartTimer();
    }

    void Update()
    {
        if (!isRunning)
            return;

        // Actualizar el tiempo basado en la velocidad ajustada
        if (countDown)
        {
            currentTime -= Time.deltaTime * timerSpeed;

            // Verificar si el timer ha llegado a 0
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isRunning = false;

                // Disparar evento de fin de timer
                if (onTimerEnd != null)
                    onTimerEnd.Invoke();

                // Reiniciar si está configurado
                if (autoReset)
                    ResetTimer();
            }
        }
        else
        {
            currentTime += Time.deltaTime * timerSpeed; // Se cuenta hacia adelante
        }

        // Disparar evento cada segundo
        int currentSecond = Mathf.FloorToInt(currentTime);
        if (currentSecond != lastSecondTick)
        {
            lastSecondTick = currentSecond;
            if (onSecondTick != null)
                onSecondTick.Invoke();
        }

        // Actualizar UI si existe
        UpdateTimerDisplay();
    }

    // Actualizar el texto UI
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Métodos públicos para controlar el timer

    /// <summary>
    /// Inicia o reanuda el timer
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// Pausa el timer
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// Reinicia el timer al valor inicial
    /// </summary>
    public void ResetTimer()
    {
        currentTime = initialTime; // Inicializa el timer en 0
        lastSecondTick = Mathf.FloorToInt(currentTime);
        UpdateTimerDisplay();

        if (autoReset)
            isRunning = true;
    }

    /// <summary>
    /// Cambia la velocidad del timer en tiempo de ejecución
    /// </summary>
    public void SetTimerSpeed(float newSpeed)
    {
        timerSpeed = Mathf.Max(0.1f, newSpeed);
    }

    /// <summary>
    /// Retorna el tiempo actual
    /// </summary>
    public float GetCurrentTime()
    {
        return currentTime;
    }

    /// <summary>
    /// Establece un nuevo tiempo inicial y resetea el timer
    /// </summary>
    public void SetNewInitialTime(float newTime)
    {
        initialTime = newTime;
        ResetTimer();
    }

    /// <summary>
    /// Añade o resta tiempo al timer actual
    /// </summary>
    public void AddTime(float timeToAdd)
    {
        currentTime += timeToAdd;
        UpdateTimerDisplay();
    }
}