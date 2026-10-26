using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("UI de Notificación")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private RectTransform notificationRect; // NUEVO: Para mover el panel
    [SerializeField] private Image characterIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image timeBar;

    [Header("Animación")]
    [SerializeField] private float velocidadAnimacion = 15f;
    [SerializeField] private float posicionXFuera = -1500f; // Dónde empieza (fuera a la izquierda)
    private float posicionXOriginal; // Dónde debe terminar (centro)

    [Header("Personajes")]
    [SerializeField] private Sprite tutoSprite;
    [SerializeField] private Sprite doomSprite;

    [Header("Configuración")]
    [SerializeField] private float duracion = 4f;
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
        
        // Guardamos la posición central que le pusiste en el Canvas
        if (notificationRect != null) posicionXOriginal = notificationRect.anchoredPosition.x;
    }

    public void MostrarNotificacion(bool fueBuenaDecision, bool fueScrollImpulsivo = false)
    {
        if (Random.value > probabilidadAparicion) return;

        bool hablaTuto = Random.value > 0.5f;

        characterIcon.sprite = hablaTuto ? tutoSprite : doomSprite;
        nameText.text = hablaTuto ? "Tuto" : "Doom";
        nameText.color = hablaTuto ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f); 

        if (fueScrollImpulsivo)
        {
            messageText.text = hablaTuto ? tutoScroll[0] : doomScroll[0];
        }
        else if (fueBuenaDecision)
        {
            string[] opciones = hablaTuto ? tutoGood : doomGood;
            messageText.text = opciones[Random.Range(0, opciones.Length)];
        }
        else
        {
            string[] opciones = hablaTuto ? tutoBad : doomBad;
            messageText.text = opciones[Random.Range(0, opciones.Length)];
        }

        StopAllCoroutines();
        StartCoroutine(RutinaNotificacion());
    }

    private IEnumerator RutinaNotificacion()
    {
        // 1. Pausamos el juego de inmediato
        Time.timeScale = 0f; 

        // 2. Teletransportamos el panel fuera de la pantalla (a la izquierda)
        if (notificationRect != null)
        {
            notificationRect.anchoredPosition = new Vector2(posicionXFuera, notificationRect.anchoredPosition.y);
        }
        
        notificationPanel.SetActive(true);

        // 3. Animamos la entrada usando unscaledDeltaTime (porque el juego está pausado)
        if (notificationRect != null)
        {
            while (Mathf.Abs(notificationRect.anchoredPosition.x - posicionXOriginal) > 1f)
            {
                float nuevaX = Mathf.Lerp(notificationRect.anchoredPosition.x, posicionXOriginal, Time.unscaledDeltaTime * velocidadAnimacion);
                notificationRect.anchoredPosition = new Vector2(nuevaX, notificationRect.anchoredPosition.y);
                yield return null;
            }
            // Aseguramos que quede exactamente en su lugar
            notificationRect.anchoredPosition = new Vector2(posicionXOriginal, notificationRect.anchoredPosition.y);
        }

        // 4. Inicia la cuenta regresiva de la barra
        float timer = duracion;
        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime; 
            if (timeBar != null) timeBar.fillAmount = timer / duracion;
            yield return null;
        }

        // 5. Desaparece de golpe y reanuda el juego
        notificationPanel.SetActive(false);
        Time.timeScale = 1f; 
    }
}