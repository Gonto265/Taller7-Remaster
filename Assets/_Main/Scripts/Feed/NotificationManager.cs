using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("UI de Notificación")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Image characterIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image timeBar; // Imagen con Image Type: Filled

    [Header("Personajes")]
    [SerializeField] private Sprite tutoSprite;
    [SerializeField] private Sprite doomSprite;

    [Header("Configuración")]
    [SerializeField] private float duracion = 4f;
    [Range(0f, 1f)]
    [SerializeField] private float probabilidadAparicion = 0.35f; // 35% de que aparezca tras una acción

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
    }

    public void MostrarNotificacion(bool fueBuenaDecision, bool fueScrollImpulsivo = false)
    {
        if (Random.value > probabilidadAparicion) return;

        bool hablaTuto = Random.value > 0.5f;

        characterIcon.sprite = hablaTuto ? tutoSprite : doomSprite;
        nameText.text = hablaTuto ? "Tuto" : "Doom";
        nameText.color = hablaTuto ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f); // Verde para Tuto, Rojo para Doom

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
        notificationPanel.SetActive(true);
        Time.timeScale = 0f; // Pausa contadores, dooming automático y videos

        float timer = duracion;
        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime; // unscaledDeltaTime avanza aunque TimeScale sea 0
            if (timeBar != null) timeBar.fillAmount = timer / duracion;
            yield return null;
        }

        Time.timeScale = 1f; // Reanuda el juego
        notificationPanel.SetActive(false);
    }
}