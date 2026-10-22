using UnityEngine;
using TMPro;

public class IdleManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float tiempoInactividad = 30f;
    [SerializeField] private float tiempoAviso = 5f;

    [Header("Aviso de inactividad")]
    [SerializeField] private GameObject idleWarningPanel;
    [SerializeField] private TMP_Text idleCountdownText;

    private GameManager gameManager;
    private float tiempoSinInteraccion;
    private bool mostrandoAviso;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        if (idleWarningPanel != null)
            idleWarningPanel.SetActive(false);
    }

    private void Update()
    {
        if (gameManager == null)
            return;

        if (gameManager.CurrentState != GameManager.GameState.Playing)
        {
            tiempoSinInteraccion = 0f;
            OcultarAviso();
            return;
        }

        tiempoSinInteraccion += Time.deltaTime;

        float tiempoInicioAviso =
            tiempoInactividad - tiempoAviso;

        // Todavía no llegamos al aviso.
        if (tiempoSinInteraccion < tiempoInicioAviso)
            return;

        // Mostrar aviso.
        if (!mostrandoAviso)
        {
            mostrandoAviso = true;

            if (idleWarningPanel != null)
                idleWarningPanel.SetActive(true);
        }

        // Tiempo restante antes del reinicio.
        float tiempoRestante =
            tiempoInactividad - tiempoSinInteraccion;

        int segundosRestantes =
            Mathf.CeilToInt(tiempoRestante);

        if (segundosRestantes < 0)
            segundosRestantes = 0;

        if (idleCountdownText != null)
        {
            if (segundosRestantes == 1)
            {
                idleCountdownText.text =
                    "Falta 1 segundo para reiniciar";
            }
            else
            {
                idleCountdownText.text =
                    $"Faltan {segundosRestantes} segundos para reiniciar";
            }
        }

        // Se acabaron los 30 segundos.
        if (tiempoSinInteraccion >= tiempoInactividad)
        {
            ReiniciarPorInactividad();
        }
    }

    public void RegistrarInteraccion()
    {
        tiempoSinInteraccion = 0f;
        OcultarAviso();
    }

    private void OcultarAviso()
    {
        mostrandoAviso = false;

        if (idleWarningPanel != null)
            idleWarningPanel.SetActive(false);
    }

    private void ReiniciarPorInactividad()
    {
        tiempoSinInteraccion = 0f;
        OcultarAviso();

        Debug.Log(
            "30 segundos sin interacción → reiniciando al tutorial."
        );

        gameManager.ReiniciarPorInactividad();
    }

    public void ConfirmarNoScrollear()
    {
        if (gameManager == null)
            return;

        if (gameManager.CurrentState != GameManager.GameState.Playing)
            return;

        tiempoSinInteraccion = 0f;
        OcultarAviso();

        Debug.Log(
            "Jugador confirmó que no quiere scrollear → final neutral."
        );

        gameManager.FinalizarPorNoScrollear();
    }
}