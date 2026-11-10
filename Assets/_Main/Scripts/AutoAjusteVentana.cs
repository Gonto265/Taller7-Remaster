using UnityEngine;

public class AutoAjusteVentana : MonoBehaviour
{
    void Start()
    {
        // Solo ejecuta esto en la build de PC
        #if UNITY_STANDALONE || UNITY_EDITOR
        
        // 1. Obtenemos el alto total del monitor del jugador
        int altoMonitor = Screen.currentResolution.height;

        // Le restamos un margen (ej. 80 píxeles) para la barra de tareas de Windows y la barra superior de la ventana
        int altoVentana = altoMonitor - 80;

        // 2. Calculamos el ancho exacto para mantener la proporción de celular (9:16)
        int anchoVentana = Mathf.RoundToInt(altoVentana * (9f / 16f));

        // 3. Aplicamos la resolución forzando el modo ventana
        Screen.SetResolution(anchoVentana, altoVentana, FullScreenMode.Windowed);
        
        #endif
    }
}