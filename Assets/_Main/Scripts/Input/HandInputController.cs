using UnityEngine;

public class HandInputController : MonoBehaviour
{
    [Header("Cursor Visual")]
    [SerializeField] private RectTransform cursorUI;
    [SerializeField] private Canvas mainCanvas;

    [Header("Configuración de Pellizco")]
    [SerializeField] private float pinchThreshold = 0.05f; // Ajusta este valor si el pellizco es muy sensible
    
    private InputManager inputManager;

    private void Awake()
    {
        inputManager = FindFirstObjectByType<InputManager>();
    }

    // Este método recibirá las posiciones normalizadas (0.0 a 1.0) desde MediaPipe
    public void ProcesarMano(Vector2 indexTip, Vector2 thumbTip)
    {
        if (inputManager == null) return;

        // 1. Convertir coordenadas normalizadas a píxeles de pantalla
        // Nota: MediaPipe tiene el origen (0,0) arriba a la izquierda, Unity lo tiene abajo a la izquierda.
        Vector2 screenPos = new Vector2(indexTip.x * Screen.width, (1f - indexTip.y) * Screen.height);

        // 2. Mover el cursor visual
        if (cursorUI != null && mainCanvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)mainCanvas.transform, 
                screenPos, 
                mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera, 
                out Vector2 localPos);
            
            cursorUI.anchoredPosition = localPos;
        }

        // 3. Calcular si está pellizcando (Click / Agarre)
        float distance = Vector2.Distance(indexTip, thumbTip);
        bool isPinching = distance < pinchThreshold;

        // 4. Enviar los datos al InputManager
        inputManager.ReceiveHandTracking(screenPos, isPinching);
    }

    // Esta función es llamada mediante SendMessage desde MediaPipe
    public void RecibirManoDesdeMediaPipe(Vector4 datosMano)
    {
        Vector2 indexTip = new Vector2(datosMano.x, datosMano.y);
        Vector2 thumbTip = new Vector2(datosMano.z, datosMano.w);
        
        ProcesarMano(indexTip, thumbTip);
    }
}