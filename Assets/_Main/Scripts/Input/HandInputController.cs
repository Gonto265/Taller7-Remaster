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
    [SerializeField] private float openThreshold = 0.40f; // Distancia para considerar la mano abierta

    [Header("Estabilización del Cursor")]
    [SerializeField] private float suavizadoCursor = 15f; 
    private Vector2 posicionSuavizada;

    [Header("Fijación Inteligente (Dwell Click en Botones)")]
    [SerializeField] private float tiempoParaClic = 1.0f; // 1 segundo sobre el botón
    
    private float tiempoHover = 0f;
    private GameObject botonMiradoActualmente = null;
    private bool clicRealizado = false; // Evita que haga 100 clics si dejas la mano ahí

    private InputManager inputManager;
    private bool wasOpenHand = false;

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
        // 2. LÓGICA DE FIJACIÓN INTELIGENTE (BOTONES)
        // =================================================
        DetectarBotonesUI(posicionSuavizada);

        // =================================================
        // 3. DETECCIÓN MATEMÁTICA DE GESTOS
        // =================================================
        
        bool isIndexPinching = Vector2.Distance(index, thumb) < pinchThreshold;
        
        // Distancia de las puntas a la muñeca para saber si está abierta
        float distIndex = Vector2.Distance(index, wrist);
        float distMiddle = Vector2.Distance(middle, wrist);
        float distRing = Vector2.Distance(ring, wrist);
        float distPinky = Vector2.Distance(pinky, wrist);

        // MANO ABIERTA: Todos los dedos lejos de la muñeca
        bool isOpenHand = distIndex > openThreshold && distMiddle > openThreshold && 
                          distRing > openThreshold && distPinky > openThreshold;

        // =================================================
        // 4. EJECUCIÓN DE ACCIONES
        // =================================================

        // ACCIÓN: LIKE (Mano Extendida)
        if (isOpenHand && !wasOpenHand)
        {
            EjecutarLike();
        }
        wasOpenHand = isOpenHand;

        // ACCIÓN: SCROLL (Mantener Índice + Pulgar y arrastrar)
        inputManager.ReceiveHandTracking(posicionSuavizada, isIndexPinching); 
    }

    private void DetectarBotonesUI(Vector2 posicionPantalla)
    {
        if (EventSystem.current == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = posicionPantalla };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject objetoTocado = null;

        // Buscamos si tocamos algo que sea interactuable (un botón)
        if (results.Count > 0)
        {
            foreach (RaycastResult result in results)
            {
                // Verifica si el objeto tiene eventos de clic (como un Button)
                GameObject posibleBoton = ExecuteEvents.GetEventHandler<IPointerClickHandler>(result.gameObject);
                if (posibleBoton != null)
                {
                    objetoTocado = posibleBoton;
                    break;
                }
            }
        }

        // Si estamos sobre un botón válido
        if (objetoTocado != null)
        {
            if (objetoTocado == botonMiradoActualmente)
            {
                // Si seguimos sobre el mismo botón y aún no hemos hecho clic
                if (!clicRealizado)
                {
                    tiempoHover += Time.deltaTime;
                    if (tiempoHover >= tiempoParaClic)
                    {
                        // ¡Pasó 1 segundo! Hacemos clic
                        ExecuteEvents.Execute(botonMiradoActualmente, pointerData, ExecuteEvents.pointerClickHandler);
                        Debug.Log("🖱️ ¡Clic automático en: " + botonMiradoActualmente.name);
                        
                        clicRealizado = true; // Bloqueamos para no hacer más clics hasta que se quite de ahí
                    }
                }
            }
            else
            {
                // Entramos a un botón nuevo
                botonMiradoActualmente = objetoTocado;
                tiempoHover = 0f;
                clicRealizado = false;
            }
        }
        else
        {
            // No estamos sobre ningún botón, o salimos de uno
            botonMiradoActualmente = null;
            tiempoHover = 0f;
            clicRealizado = false;
        }
    }

    private void EjecutarLike()
    {
        ShortInteraction shortActivo = Object.FindFirstObjectByType<ShortInteraction>();
        if (shortActivo != null)
        {
            shortActivo.Like(); 
            Debug.Log("🖐️ ¡Like alternado por Mano Extendida!");
        }
    }

    // Recuerda que esta función la sigue llamando MediaPipe con el Array de 12 datos
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