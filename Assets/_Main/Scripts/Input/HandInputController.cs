using UnityEngine;
using UnityEngine.EventSystems; // NUEVO: Requerido para interactuar con la UI
using System.Collections.Generic; // NUEVO: Requerido para las listas de resultados

public class HandInputController : MonoBehaviour
{
    [Header("Cursor Visual")]
    [SerializeField] private RectTransform cursorUI;
    [SerializeField] private Canvas mainCanvas;

    [Header("Configuración de Pellizco")]
    [SerializeField] private float pinchThreshold = 0.05f; 
    
    private InputManager inputManager;
    private bool wasPinching = false; // Control para saber si acaba de iniciar el pellizco

    private void Awake()
    {
        inputManager = Object.FindFirstObjectByType<InputManager>();
    }

    public void ProcesarMano(Vector2 indexTip, Vector2 thumbTip)
    {
        if (inputManager == null) return;

        // 1. Convertir coordenadas
        Vector2 screenPos = new Vector2(indexTip.x * Screen.width, (1f - indexTip.y) * Screen.height);

        // 2. Mover el cursor
        if (cursorUI != null && mainCanvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)mainCanvas.transform, 
                screenPos, 
                mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera, 
                out Vector2 localPos);
            
            cursorUI.anchoredPosition = localPos;
        }

        // 3. Calcular pellizco
        float distance = Vector2.Distance(indexTip, thumbTip);
        bool isPinching = distance < pinchThreshold;

        // =================================================
        // NUEVO: SISTEMA DE CLIC EN BOTONES DE LA UI
        // =================================================
        if (isPinching && !wasPinching)
        {
            // Solo hacemos clic en el instante exacto en que se juntan los dedos
            SimularClicUI(screenPos);
        }
        wasPinching = isPinching; // Guardamos el estado para no repetir el clic si mantiene cerrado
        // =================================================

        // 4. Enviar los datos al InputManager (para que se encargue del Swipe y los Likes)
        inputManager.ReceiveHandTracking(screenPos, isPinching);
    }

    // Esta función dispara el rayo contra la UI
    private void SimularClicUI(Vector2 posicionPantalla)
    {
        if (EventSystem.current == null) return;

        // Creamos un evento de puntero en la posición del cursor
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = posicionPantalla
        };

        // Disparamos el rayo y guardamos lo que toque en una lista
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            foreach (RaycastResult result in results)
            {
                // Disparamos la acción de "Clic" en el objeto tocado
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                
                // Rompemos el ciclo para hacer clic solo en el elemento más al frente (evita pulsar botones ocultos atrás)
                break;
            }
        }
    }

    public void RecibirManoDesdeMediaPipe(Vector4 datosMano)
    {
        Vector2 indexTip = new Vector2(datosMano.x, datosMano.y);
        Vector2 thumbTip = new Vector2(datosMano.z, datosMano.w);
        
        ProcesarMano(indexTip, thumbTip);
    }
}