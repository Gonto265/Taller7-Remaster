using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("UI de Notificación")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private RectTransform notificationRect; 
    [SerializeField] private Image characterIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image timeBar;

    [Header("Animación")]
    [SerializeField] private float velocidadAnimacion = 15f;
    [SerializeField] private float posicionXFuera = -1500f; 
    private float posicionXOriginal; 

    [Header("Sprites de Personajes (Estados)")]
    [SerializeField] private Sprite tutoFeliz;
    [SerializeField] private Sprite tutoTriste;
    [SerializeField] private Sprite doomFeliz;
    [SerializeField] private Sprite doomTriste;

    [Header("Configuración")]
    [SerializeField] private float duracion = 4f;
    [SerializeField] private float retrasoAparicion = 0.5f; 
    [Range(0f, 1f)]
    [SerializeField] private float probabilidadAparicion = 0.35f; 

    private readonly string[] tutoGood = { "¡Bien hecho! Cuestionar la información es clave.", "¡Así se hace! No caíste en la trampa.", "¡Excelente! Investigar siempre ayuda." };
    private readonly string[] doomGood = { "¡Ugh! Otra vez siendo responsable. Qué aburrido.", "Me arruinaste la jugada. Deja de pensar tanto.", "¡Basta de buenas decisiones! Quiero caos." };
    private readonly string[] tutoBad = { "¡Cuidado! Esa decisión alimenta el amarillismo.", "Piénsalo mejor a la próxima, confío en ti.", "Fíjate bien, esa información no parece confiable." };
    private readonly string[] doomBad = { "¡Jaja! ¡Excelente decisión, más basura al feed!", "¡Me encanta! Tú sí que sabes cómo arruinar todo.", "¡Sí! ¡No mires los detalles, solo actúa!" };
    private readonly string[] tutoScroll = { "¡Oye, respira! Deslizar tan rápido no te deja pensar." };
    private readonly string[] doomScroll = { "¡Sigue deslizando! Tu falta de atención es mi energía." };

    private bool fueInterrumpidoPorScroll = false;
    private InputManager inputManager;

    private void Awake()
    {
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (notificationRect != null) posicionXOriginal = notificationRect.anchoredPosition.x;
        
        inputManager = Object.FindFirstObjectByType<InputManager>();
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnScrollDetectado += InterrumpirNotificacion;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnScrollDetectado -= InterrumpirNotificacion;
        }
    }

    private void InterrumpirNotificacion()
    {
        if (notificationPanel != null && notificationPanel.activeSelf)
        {
            fueInterrumpidoPorScroll = true;
        }
    }

    public void MostrarNotificacion(bool fueBuenaDecision, bool fueScrollImpulsivo = false)
    {
        if (Random.value > probabilidadAparicion) return;
        StartCoroutine(RutinaEsperarYMostrar(fueBuenaDecision, fueScrollImpulsivo));
    }

    private IEnumerator RutinaEsperarYMostrar(bool fueBuenaDecision, bool fueScrollImpulsivo)
    {
        yield return new WaitForSeconds(retrasoAparicion);

        StopAllCoroutines(); 
        fueInterrumpidoPorScroll = false; 
        StartCoroutine(RutinaNotificacion(fueBuenaDecision, fueScrollImpulsivo));
    }

    private IEnumerator RutinaNotificacion(bool fueBuenaDecision, bool fueScrollImpulsivo)
    {
        bool hablaTuto = Random.value > 0.5f;

        nameText.text = hablaTuto ? "Tuto" : "Doom";
        nameText.color = hablaTuto ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f); 

        if (fueScrollImpulsivo)
        {
            messageText.text = hablaTuto ? tutoScroll[0] : doomScroll[0];
            characterIcon.sprite = hablaTuto ? tutoTriste : doomFeliz;
        }
        else if (fueBuenaDecision)
        {
            string[] opciones = hablaTuto ? tutoGood : doomGood;
            messageText.text = opciones[Random.Range(0, opciones.Length)];
            characterIcon.sprite = hablaTuto ? tutoFeliz : doomTriste;
        }
        else
        {
            string[] opciones = hablaTuto ? tutoBad : doomBad;
            messageText.text = opciones[Random.Range(0, opciones.Length)];
            characterIcon.sprite = hablaTuto ? tutoTriste : doomFeliz;
        }

        if (notificationRect != null)
        {
            notificationRect.anchoredPosition = new Vector2(posicionXFuera, notificationRect.anchoredPosition.y);
        }
        
        notificationPanel.SetActive(true);

        // --- ANIMACIÓN DE ENTRADA ---
        if (notificationRect != null)
        {
            while (Mathf.Abs(notificationRect.anchoredPosition.x - posicionXOriginal) > 1f)
            {
                float nuevaX = Mathf.Lerp(notificationRect.anchoredPosition.x, posicionXOriginal, Time.deltaTime * velocidadAnimacion);
                notificationRect.anchoredPosition = new Vector2(nuevaX, notificationRect.anchoredPosition.y);
                yield return null;
            }
            notificationRect.anchoredPosition = new Vector2(posicionXOriginal, notificationRect.anchoredPosition.y);
        }

        // --- TIEMPO DE ESPERA (Se rompe si el jugador hace scroll) ---
        float timer = duracion;
        while (timer > 0f && !fueInterrumpidoPorScroll)
        {
            timer -= Time.deltaTime; 
            if (timeBar != null) timeBar.fillAmount = timer / duracion;
            yield return null;
        }

        // --- NUEVO: ANIMACIÓN DE SALIDA ---
        // Si fue interrumpido por el scroll, sale un poco más rápido (x1.5) para desaparecer pronto
        float velocidadSalida = fueInterrumpidoPorScroll ? velocidadAnimacion * 1.5f : velocidadAnimacion;

        if (notificationRect != null)
        {
            while (Mathf.Abs(notificationRect.anchoredPosition.x - posicionXFuera) > 1f)
            {
                float nuevaX = Mathf.Lerp(notificationRect.anchoredPosition.x, posicionXFuera, Time.deltaTime * velocidadSalida);
                notificationRect.anchoredPosition = new Vector2(nuevaX, notificationRect.anchoredPosition.y);
                yield return null;
            }
        }

        notificationPanel.SetActive(false);
        fueInterrumpidoPorScroll = false; 
    }
}