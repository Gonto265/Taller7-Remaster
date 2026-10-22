using UnityEngine;
using System.Collections;

public class SwipeAnimator : MonoBehaviour
{
    [Header("Configuración visual")]
    [SerializeField] private RectTransform panelRect; // Aquí arrastraremos tu ShortContainer
    [SerializeField] private float alturaPantalla = 1920f;
    [SerializeField] private float velocidadAnimacion = 15f;

    private InputManager inputManager;
    private FeedManager feedManager;
    private bool estaAnimando;
    private float posicionYOriginal;

    private void Awake()
    {
        inputManager = FindFirstObjectByType<InputManager>();
        feedManager = FindFirstObjectByType<FeedManager>();
        
        if (panelRect != null)
        {
            posicionYOriginal = panelRect.anchoredPosition.y;
        }
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnDrag += ManejarArrastre;
            inputManager.SwipeUp += EjecutarAnimacionSwipe;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnDrag -= ManejarArrastre;
            inputManager.SwipeUp -= EjecutarAnimacionSwipe;
        }
    }

    private void ManejarArrastre(float deltaY)
    {
        if (estaAnimando || panelRect == null) return;

        // Si el usuario soltó el dedo sin fuerza (deltaY == 0), devolvemos el panel al centro suavemente
        if (deltaY == 0f)
        {
            StartCoroutine(AnimarHacia(posicionYOriginal));
        }
        else
        {
            // El panel sigue al dedo (limitamos para que no baje, solo suba o se quede en el centro)
            float nuevaY = posicionYOriginal + deltaY;
            if (nuevaY < posicionYOriginal) nuevaY = posicionYOriginal; // Opcional: Evita que el video se arrastre hacia abajo
            
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, nuevaY);
        }
    }

    private void EjecutarAnimacionSwipe()
    {
        if (estaAnimando || panelRect == null) return;
        StartCoroutine(RutinaSwipe());
    }

    private IEnumerator AnimarHacia(float objetivoY)
    {
        estaAnimando = true;
        while (Mathf.Abs(panelRect.anchoredPosition.y - objetivoY) > 1f)
        {
            float nuevaY = Mathf.Lerp(panelRect.anchoredPosition.y, objetivoY, Time.deltaTime * velocidadAnimacion);
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, nuevaY);
            yield return null;
        }
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, objetivoY);
        estaAnimando = false;
    }

    private IEnumerator RutinaSwipe()
    {
        estaAnimando = true;

        // 1. Animar el panel actual hasta que salga por la parte superior de la pantalla
        while (panelRect.anchoredPosition.y < alturaPantalla - 10f)
        {
            float nuevaY = Mathf.Lerp(panelRect.anchoredPosition.y, alturaPantalla, Time.deltaTime * velocidadAnimacion);
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, nuevaY);
            yield return null;
        }

        // 2. Teletransportar el panel a la parte inferior (fuera de cámara)
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, -alturaPantalla);

        // 3. AHORA cargamos el siguiente video (el panel está invisible en este momento)
        if (feedManager != null)
        {
            feedManager.Next();
        }

        // Pequeña pausa opcional para dejar que el VideoPlayer haga el Prepare() sin tirones visuales
        yield return new WaitForSeconds(0.05f);

        // 4. Animar el panel desde abajo de vuelta a su posición original (el centro)
        while (Mathf.Abs(panelRect.anchoredPosition.y - posicionYOriginal) > 1f)
        {
            float nuevaY = Mathf.Lerp(panelRect.anchoredPosition.y, posicionYOriginal, Time.deltaTime * velocidadAnimacion);
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, nuevaY);
            yield return null;
        }

        // Asegurarnos de que quede exactamente en el centro
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, posicionYOriginal);
        estaAnimando = false;
    }
}