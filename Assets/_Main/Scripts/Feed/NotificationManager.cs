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
    [SerializeField] private float retrasoAparicion = 0.5f; // Espera a que pase el destello visual (flash)
    [Range(0f, 1f)]
    [SerializeField] private float probabilidadAparicion = 0.35f; 

    // Banco de frases
    private readonly string[] tutoGood = { "¡Bien hecho! Cuestionar la información es clave.", "¡Así se hace! No caíste en la trampa.", "¡Excelente! Investigar siempre ayuda." };
    private readonly string[] doomGood = { "¡Ugh! Otra vez siendo responsable. Qué aburrido.", "Me arruinaste la jugada. Deja de pensar tanto.", "¡Basta de buenas decisiones! Quiero caos." };
    
    private readonly string[] tutoBad = { "¡Cuidado! Esa decisión alimenta el amarillismo.", "Piénsalo mejor a la próxima, confío en ti.", "Fíjate bien, esa información no parece confiable." };
    private readonly string[] doomBad = { "¡Jaja! ¡Excelente decisión, más basura al feed!", "¡Me encanta! Tú sí que sabes cómo arruinar todo.", "¡Sí! ¡No mires los detalles, solo actúa!" };

    private readonly string[] tutoScroll = { "¡Oye, respira! Deslizar tan rápido no te deja pensar." };
    private readonly string[] doomScroll = { "¡Sigue deslizando! Tu falta de atención es mi energía." };

    private void Awake()
    {
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (notificationRect != null) posicionXOriginal = notificationRect.anchoredPosition.x;
    }

    public void MostrarNotificacion(bool fueBuenaDecision, bool fueScrollImpulsivo = false)
    {
        if (Random.value > probabilidadAparicion) return;

        // En lugar de pausar de golpe, iniciamos una rutina que primero espera
        StartCoroutine(RutinaEsperarYMostrar(fueBuenaDecision, fueScrollImpulsivo));
    }

    private IEnumerator RutinaEsperarYMostrar(bool fueBuenaDecision, bool fueScrollImpulsivo)
    {
        // 1. Esperamos el tiempo del destello para no congelar la pantalla de un solo color
        yield return new WaitForSeconds(retrasoAparicion);

        // 2. Ahora sí, lanzamos la notificación (esto detiene otras si justo se cruzaron)
        StopAllCoroutines(); 
        StartCoroutine(RutinaNotificacion(fueBuenaDecision, fueScrollImpulsivo));
    }

    private IEnumerator RutinaNotificacion(bool fueBuenaDecision, bool fueScrollImpulsivo)
    {
        // --- ASIGNACIÓN DE TEXTOS E ICONOS ---
        bool hablaTuto = Random.value > 0.5f;

        nameText.text = hablaTuto ? "Tuto" : "Doom";
        nameText.color = hablaTuto ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f); 

        if (fueScrollImpulsivo)
        {
            messageText.text = hablaTuto ? tutoScroll[0] : doomScroll[0];
            // Scroll impulsivo = Malo (Tuto triste, Doom feliz)
            characterIcon.sprite = hablaTuto ? tutoTriste : doomFeliz;
        }
        else if (fueBuenaDecision)
        {
            string[] opciones = hablaTuto ? tutoGood : doomGood;
            messageText.text = opciones[Random.Range(0, opciones.Length)];
            // Buena decisión = Tuto feliz, Doom enojado/triste
            characterIcon.sprite = hablaTuto ? tutoFeliz : doomTriste;
        }
        else
        {
            string[] opciones = hablaTuto ? tutoBad : doomBad;
            messageText.text = opciones[Random.Range(0, opciones.Length)];
            // Mala decisión = Tuto triste, Doom feliz
            characterIcon.sprite = hablaTuto ? tutoTriste : doomFeliz;
        }

        // --- PAUSA Y ANIMACIÓN ---
        Time.timeScale = 0f; 

        if (notificationRect != null)
        {
            notificationRect.anchoredPosition = new Vector2(posicionXFuera, notificationRect.anchoredPosition.y);
        }
        
        notificationPanel.SetActive(true);

        if (notificationRect != null)
        {
            while (Mathf.Abs(notificationRect.anchoredPosition.x - posicionXOriginal) > 1f)
            {
                float nuevaX = Mathf.Lerp(notificationRect.anchoredPosition.x, posicionXOriginal, Time.unscaledDeltaTime * velocidadAnimacion);
                notificationRect.anchoredPosition = new Vector2(nuevaX, notificationRect.anchoredPosition.y);
                yield return null;
            }
            notificationRect.anchoredPosition = new Vector2(posicionXOriginal, notificationRect.anchoredPosition.y);
        }

        float timer = duracion;
        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime; 
            if (timeBar != null) timeBar.fillAmount = timer / duracion;
            yield return null;
        }

        notificationPanel.SetActive(false);
        Time.timeScale = 1f; 
    }
}