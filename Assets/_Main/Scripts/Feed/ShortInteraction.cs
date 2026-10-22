using UnityEngine;
using System.Collections;

public class ShortInteraction : MonoBehaviour
{
    private bool liked;
    private bool reported;

    // Cada tipo de búsqueda se puede aplicar solo una vez
    // por Short.
    private bool busquedaNegativaUsada;
    private bool busquedaNeutralUsada;
    private bool busquedaPositivaUsada;

    private float tiempoInicioShort;

    private const float TIEMPO_SCROLL_IMPULSIVO = 1f;

    private bool isTutorial;

    private ShortData shortData;
    private DoomingManager doomingManager;
    private IdleManager idleManager;
    private GameManager gameManager;

    [Header("Feedback Visual")]
    [SerializeField] private UnityEngine.UI.Image imagenFeedbackPositivo;
    [SerializeField] private UnityEngine.UI.Image imagenFeedbackNegativo;
    [SerializeField] private float duracionFlash = 0.5f;

    private Coroutine flashCoroutine;    

    [SerializeField] private GameObject reportConfirmation;

    [SerializeField] private UnityEngine.UI.Image likeIcon;
    [SerializeField] private Sprite likeVacio;
    [SerializeField] private Sprite likeLleno;
    
    [SerializeField] private UnityEngine.UI.Image reportIcon;
    [SerializeField] private Sprite reportVacio;
    [SerializeField] private Sprite reportLleno;

    // Guardamos cuánto efecto produjo realmente el Like.
    // Esto permite revertirlo correctamente incluso
    // si el valor estaba cerca del mínimo.
    private float likeCambioSAplicado;
    private float likeCambioFAplicado;

    public bool Liked => liked;
    public bool Reported => reported;

    private void Awake()
    {       
        idleManager = FindFirstObjectByType<IdleManager>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void Initialize(
        ShortData data,
        bool tutorial,
        DoomingManager manager)
    {
        tiempoInicioShort = Time.time;
        shortData = data;
        isTutorial = tutorial;
        doomingManager = manager;

        liked = false;
        ActualizarIconos();
        reported = false;

        ActualizarIconos();

        busquedaNegativaUsada = false;
        busquedaNeutralUsada = false;
        busquedaPositivaUsada = false;

        likeCambioSAplicado = 0f;
        likeCambioFAplicado = 0f;

        if (reportConfirmation != null)
        {
            reportConfirmation.SetActive(false);
        }

        Debug.Log(
            $"ShortInteraction inicializado → {shortData.name}, " +
            $"Tutorial: {isTutorial}"
        );
    }

    private void ActualizarIconos()
    {
        if (likeIcon != null)
        {
            likeIcon.sprite = liked ? likeLleno : likeVacio;
        }
    
        if (reportIcon != null)
        {
            reportIcon.sprite = reported ? reportLleno : reportVacio;
        }
    }

    private bool PuedeInteractuar()
    {
        if (gameManager == null)
            return false;
    
        return gameManager.CurrentState == GameManager.GameState.Tutorial ||
               gameManager.CurrentState == GameManager.GameState.Playing;
    }

    // =========================================================
    // LIKE
    // =========================================================

    public void Like()
    {
        if (!PuedeInteractuar())
           return;

        if (shortData == null || doomingManager == null)
            return;

        if (idleManager != null)
            idleManager.RegistrarInteraccion();

        // Quitar Like
        if (liked)
        {
            liked = false;
            ActualizarIconos();

            if (isTutorial)
            {
                Debug.Log("Like quitado en tutorial. No modifica Dooming.");
                return;
            }

            // Revertimos exactamente el efecto que produjo
            // el Like anterior.
            doomingManager.ModificarDooming(
                -likeCambioSAplicado,
                -likeCambioFAplicado
            );

            Debug.Log(
                $"Like quitado → S: {-likeCambioSAplicado}, " +
                $"F: {-likeCambioFAplicado}"
            );

            likeCambioSAplicado = 0f;
            likeCambioFAplicado = 0f;

            return;
        }

        // Dar Like
        liked = true;
        ActualizarIconos();

        if (isTutorial)
        {
            Debug.Log("Like registrado en tutorial. No modifica Dooming.");
            return;
        }

        int cambioS = ObtenerCambioLikeSensacionalismo();
        int cambioF = ObtenerCambioLikeFalsedad();

        Vector2 cambioReal = doomingManager.ModificarDooming(
            cambioS,
            cambioF
        );

        // Si la suma de los cambios es menor o igual a 0, fue una buena decisión.
        bool fueBuenaDecision = (cambioReal.x + cambioReal.y) <= 0;
        MostrarFlash(fueBuenaDecision);

        likeCambioSAplicado = cambioReal.x;
        likeCambioFAplicado = cambioReal.y;

        Debug.Log(
            $"Like aplicado → S: {cambioReal.x}, F: {cambioReal.y}"
        );
    }

    // =========================================================
    // REPORT
    // =========================================================

    public void Report()
    {
        if (!PuedeInteractuar())
           return;

        if (reported)
            return;

        if (idleManager != null)
            idleManager.RegistrarInteraccion();

        Debug.Log("Report solicitado. Esperando confirmación.");

        if (reportConfirmation != null)
        {
            reportConfirmation.SetActive(true);
        }
    }

    public void CancelarReport()
    {      
        if (idleManager != null)
            idleManager.RegistrarInteraccion();

        if (reportConfirmation != null)
        {
            reportConfirmation.SetActive(false);
        }

        Debug.Log("Reporte cancelado.");
    }

    public void ConfirmarReport(bool reporteCorrecto)
    {
        if (!PuedeInteractuar())
           return;

        if (reported)
            return;
    
        if (idleManager != null)
            idleManager.RegistrarInteraccion();
    
        reported = true;
        ActualizarIconos();
    
        if (reportConfirmation != null)
        {
            reportConfirmation.SetActive(false);
        }
    
        if (isTutorial)
        {
            Debug.Log(
                "Report confirmado en tutorial. No modifica Dooming."
            );
            return;
        }
    
        int cambioS = ObtenerCambioReport(reporteCorrecto);
        doomingManager.ModificarSensacionalismo(cambioS);

        // --- NUEVA LÓGICA DE FEEDBACK VISUAL ---
        if (cambioS > 0)
        {
            MostrarFlash(false); // Aumenta el Dooming = Mala decisión (Rojo/Negativo)
        }
        else if (cambioS < 0)
        {
            MostrarFlash(true);  // Reduce el Dooming = Buena decisión (Verde/Positivo)
        }
        // Si cambioS es exactamente 0 (caso 2), simplemente no llamamos a MostrarFlash()
        // ---------------------------------------
    
        Debug.Log(
            $"Report confirmado → S: {cambioS}"
        );
    }

    public void ConfirmarReportCorrecto()
    {
        ConfirmarReport(true);
    }

    // =========================================================
    // SEARCH
    // =========================================================

    public void Search(int indiceOpcion)
    {
        if (!PuedeInteractuar())
           return;

        if (shortData == null || doomingManager == null)
            return;

        if (shortData.searchOptions == null)
            return;

        if (indiceOpcion < 0 ||
            indiceOpcion >= shortData.searchOptions.Length)
            return;

        if (idleManager != null)
            idleManager.RegistrarInteraccion();

        SearchOption opcion = shortData.searchOptions[indiceOpcion];

        // Comprobamos si ESTE TIPO ya fue utilizado.
        if (BusquedaYaUtilizada(opcion.tipo))
        {
            Debug.Log(
                $"Búsqueda {opcion.tipo} ya utilizada en este Short. " +
                "No modifica Dooming."
            );

            return;
        }

        // La marcamos como utilizada antes de aplicar el efecto.
        MarcarBusquedaComoUtilizada(opcion.tipo);

        if (isTutorial)
        {
            Debug.Log(
                $"Búsqueda registrada en tutorial → {opcion.tipo}. " +
                "No modifica Dooming."
            );

            return;
        }

        float cambioF = opcion.ObtenerEfectoFalsedad();

        doomingManager.ModificarFalsedad(cambioF);

        if (opcion.tipo == SearchOption.SearchType.Positiva)
        {
            MostrarFlash(true);
        }
        else if (opcion.tipo == SearchOption.SearchType.Negativa)
        {
            MostrarFlash(false);
        }

        Debug.Log(
            $"Búsqueda → {opcion.tipo}, F: {cambioF}"
        );
    }

    private bool BusquedaYaUtilizada(SearchOption.SearchType tipo)
    {
        switch (tipo)
        {
            case SearchOption.SearchType.Negativa:
                return busquedaNegativaUsada;

            case SearchOption.SearchType.Neutral:
                return busquedaNeutralUsada;

            case SearchOption.SearchType.Positiva:
                return busquedaPositivaUsada;

            default:
                return false;
        }
    }

    private void MarcarBusquedaComoUtilizada(
        SearchOption.SearchType tipo)
    {
        switch (tipo)
        {
            case SearchOption.SearchType.Negativa:
                busquedaNegativaUsada = true;
                break;

            case SearchOption.SearchType.Neutral:
                busquedaNeutralUsada = true;
                break;

            case SearchOption.SearchType.Positiva:
                busquedaPositivaUsada = true;
                break;
        }
    }

    // =========================================================
    // EFECTOS
    // =========================================================

    private int ObtenerCambioLikeSensacionalismo()
    {
        switch (shortData.sensacionalismo)
        {
            case 1: return -2;
            case 2: return 2;
            case 3: return 5;
            default: return 0;
        }
    }

    private int ObtenerCambioLikeFalsedad()
    {
        switch (shortData.falsedad)
        {
            case 1: return -2;
            case 2: return 2;
            case 3: return 3;
            default: return 0;
        }
    }

    private int ObtenerCambioReport(bool reporteCorrecto)
    {
        if (!reporteCorrecto)
            return 3;

        switch (shortData.sensacionalismo)
        {
            case 1: return 6;
            case 2: return 0;
            case 3: return -6;
            default: return 0;
        }
    }

    public void ProcesarScroll()
    {
        if (!PuedeInteractuar())
           return;

        if (isTutorial)
        {
            Debug.Log("Scroll del tutorial. No modifica Dooming.");
            return;
        }
    
        float tiempoTranscurrido =
            Time.time - tiempoInicioShort;
    
        if (tiempoTranscurrido < TIEMPO_SCROLL_IMPULSIVO)
        {
            doomingManager.ModificarDooming(4f, 4f);
    
            Debug.Log(
                $"Scroll impulsivo → +4 S, +4 F " +
                $"(tiempo: {tiempoTranscurrido:F2}s)"
            );
        }
        else
        {
            Debug.Log(
                $"Scroll normal → sin penalización " +
                $"(tiempo: {tiempoTranscurrido:F2}s)"
            );
        }
    }

    private void MostrarFlash(bool decisionPositiva)
    {
        // Nos aseguramos de apagar ambas antes de empezar
        if (imagenFeedbackPositivo != null) imagenFeedbackPositivo.gameObject.SetActive(false);
        if (imagenFeedbackNegativo != null) imagenFeedbackNegativo.gameObject.SetActive(false);

        // Elegimos qué imagen usar
        UnityEngine.UI.Image imagenActiva = decisionPositiva ? imagenFeedbackPositivo : imagenFeedbackNegativo;
        
        if (imagenActiva == null) return;
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(RutinaFlash(imagenActiva));
    }

    private System.Collections.IEnumerator RutinaFlash(UnityEngine.UI.Image imagen)
    {
        imagen.gameObject.SetActive(true);
        
        // Tomamos el color actual (por si la imagen ya tiene un tinte desde el inspector)
        Color colorInicio = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 1f);
        Color colorTransparente = new Color(imagen.color.r, imagen.color.g, imagen.color.b, 0f);
        
        imagen.color = colorInicio;
        float tiempo = 0f;
        
        while (tiempo < duracionFlash)
        {
            tiempo += Time.deltaTime;
            imagen.color = Color.Lerp(colorInicio, colorTransparente, tiempo / duracionFlash);
            yield return null; // Esperamos al siguiente frame
        }
        
        imagen.gameObject.SetActive(false);
    }
}