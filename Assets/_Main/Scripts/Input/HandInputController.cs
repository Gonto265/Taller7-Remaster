using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HandInputController : MonoBehaviour
{
    [Header("Cursor Visual")]
    [SerializeField] private RectTransform cursorUI;
    [SerializeField] private Canvas mainCanvas;

    [Header("Configuración de Gestos")]
    [SerializeField] private float pinchThreshold = 0.05f; 

    [Header("Estabilización del Cursor")]
    [SerializeField] private float suavizadoCursor = 15f; 
    private Vector2 posicionSuavizada;

    [Header("Fijación Inteligente (Dwell Click)")]
    [SerializeField] private float tiempoParaClic = 0.4f; 
    
    private float tiempoHover = 0f;
    private GameObject botonMiradoActualmente = null;
    private bool clicRealizado = false; 

    [Header("Like (Doble Pellizco)")]
    [SerializeField] private float tiempoMaxDoblePellizco = 0.5f; // Tiempo máximo para hacer el doble tap
    [SerializeField] private float tiempoMinAntiParpadeo = 0.15f; // Filtro: Evita "falsos dobles clics" por temblor de cámara
    
    private float ultimoTiempoPellizco = -1f;
    private bool wasIndexPinching = false;

    private InputManager inputManager;

    private void Awake()
    {
        inputManager = Object.FindFirstObjectByType<InputManager>();
        posicionSuavizada = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    public void ProcesarManoMultiple(Vector2 wrist, Vector2 thumb, Vector2 index, Vector2 middle, Vector2 ring, Vector2 pinky)
    {
        if (inputManager == null) return;

        // 1. Suavizado y Movimiento del Cursor
        Vector2 targetScreenPos = new Vector2(index.x * Screen.width, (1f - index.y) * Screen.height);
        posicionSuavizada = Vector2.Lerp(posicionSuavizada, targetScreenPos, Time.deltaTime * suavizadoCursor);

        if (cursorUI != null && mainCanvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)mainCanvas.transform, 
                posicionSuavizada, 
                mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera, 
                out Vector2 localPos);
            
            cursorUI.anchoredPosition = localPos;
        }

        // =================================================
        // 2. LÓGICA DE FIJACIÓN (BOTONES UI)
        // =================================================
        DetectarBotonesUI(posicionSuavizada);

        // =================================================
        // 3. DETECCIÓN DE PELLIZCO (SCROLL Y LIKE)
        // =================================================
        bool isIndexPinching = Vector2.Distance(index, thumb) < pinchThreshold;
        
        // Lógica de Doble Pellizco con Filtro
        if (isIndexPinching && !wasIndexPinching) // Justo en el frame que pellizca
        {
            float tiempoActual = Time.time;
            float diferencia = tiempoActual - ultimoTiempoPellizco;

            // Verificamos si está en el rango correcto (ni un error de cámara, ni demasiado lento)
            if (diferencia > tiempoMinAntiParpadeo && diferencia <= tiempoMaxDoblePellizco)
            {
                EjecutarLike();
                ultimoTiempoPellizco = 0f; // Reiniciamos para evitar un triple click
            }
            else if (diferencia > tiempoMaxDoblePellizco)
            {
                // Es el primer pellizco intencional de una posible secuencia
                ultimoTiempoPellizco = tiempoActual;
            }
        }
        wasIndexPinching = isIndexPinching;

        // Enviamos al InputManager para el Scroll (mantener presionado)
        inputManager.ReceiveHandTracking(posicionSuavizada, isIndexPinching); 
    }

    private void EjecutarLike()
    {
        ShortInteraction shortActivo = Object.FindFirstObjectByType<ShortInteraction>();
        if (shortActivo != null)
        {
            shortActivo.Like();
            Debug.Log("❤️ ¡Like activado por Doble Pellizco!");
        }
    }

    private void DetectarBotonesUI(Vector2 posicionPantalla)
    {
        if (EventSystem.current == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = posicionPantalla };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject objetoTocado = null;

        if (results.Count > 0)
        {
            foreach (RaycastResult result in results)
            {
                GameObject posibleBoton = ExecuteEvents.GetEventHandler<IPointerClickHandler>(result.gameObject);
                if (posibleBoton != null)
                {
                    objetoTocado = posibleBoton;
                    break;
                }
            }
        }

        if (objetoTocado != null)
        {
            if (objetoTocado == botonMiradoActualmente)
            {
                if (!clicRealizado)
                {
                    tiempoHover += Time.deltaTime;
                    if (tiempoHover >= tiempoParaClic)
                    {
                        ExecuteEvents.Execute(botonMiradoActualmente, pointerData, ExecuteEvents.pointerClickHandler);
                        Debug.Log("🖱️ ¡Clic automático en: " + botonMiradoActualmente.name);
                        clicRealizado = true; 
                    }
                }
            }
            else
            {
                botonMiradoActualmente = objetoTocado;
                tiempoHover = 0f;
                clicRealizado = false;
            }
        }
        else
        {
            botonMiradoActualmente = null;
            tiempoHover = 0f;
            clicRealizado = false;
        }
    }

    public void RecibirManoDesdeMediaPipe(float[] datos)
    {
        if (datos == null || datos.Length < 12) return;

        Vector2 wrist = new Vector2(datos[0], datos[1]);
        Vector2 thumb = new Vector2(datos[2], datos[3]);
        Vector2 index = new Vector2(datos[4], datos[5]);
        Vector2 middle = new Vector2(datos[6], datos[7]);
        Vector2 ring = new Vector2(datos[8], datos[9]);
        Vector2 pinky = new Vector2(datos[10], datos[11]);
        
        ProcesarManoMultiple(wrist, thumb, index, middle, ring, pinky);
    }
}