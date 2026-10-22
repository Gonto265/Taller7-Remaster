using UnityEngine;

public class FeedManager : MonoBehaviour
{
    private GameManager gameManager;
    private FeedSelector feedSelector;
    private ShortView shortView;
    private ShortInteraction shortInteraction;
    private DoomingManager doomingManager;

    [Header("Tutorial")]
    [SerializeField] private ShortData tutorialShort;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        feedSelector = FindFirstObjectByType<FeedSelector>();
        shortView = FindFirstObjectByType<ShortView>();
        shortInteraction = FindFirstObjectByType<ShortInteraction>();
        doomingManager = FindFirstObjectByType<DoomingManager>();
    }

    private void Start()
    {
        CargarTutorial();
    }

    public void Next()
    {
        if (shortInteraction != null)
        {
            shortInteraction.ProcesarScroll();
        }

        CargarSiguienteShort();
    }

    private void CargarTutorial()
    {
        if (tutorialShort == null)
        {
            Debug.LogError(
                "No se asignó el ShortData del Tutorial en FeedManager."
            );
            return;
        }

        gameManager.SetCurrentShort(tutorialShort);

        if (shortInteraction != null && doomingManager != null)
        {
            shortInteraction.Initialize(
                tutorialShort,
                true,
                doomingManager
            );
        }
        else
        {
            Debug.LogError(
                "No se encontró ShortInteraction o DoomingManager."
            );
        }

        if (shortView != null)
        {
            shortView.MostrarShort(tutorialShort);
        }
        else
        {
            Debug.LogError("No se encontró ShortView.");
        }

        Debug.Log(
            $"Tutorial cargado → {tutorialShort.name}"
        );
    }

    private void CargarSiguienteShort()
    {
        if (feedSelector == null)
        {
            Debug.LogError("No se encontró FeedSelector.");
            return;
        }

        ShortData siguienteShort = feedSelector.ObtenerSiguiente();

        if (siguienteShort == null)
        {
            Debug.Log("No hay más Shorts disponibles.");
            return;
        }

        gameManager.SetCurrentShort(siguienteShort);

        if (shortInteraction != null && doomingManager != null)
        {
            bool esTutorial =
                gameManager.CurrentState == GameManager.GameState.Tutorial;

            shortInteraction.Initialize(
                siguienteShort,
                esTutorial,
                doomingManager
            );
        }
        else
        {
            Debug.LogError(
                "No se encontró ShortInteraction o DoomingManager."
            );
        }

        if (shortView != null)
        {
            shortView.MostrarShort(siguienteShort);
        }
        else
        {
            Debug.LogError("No se encontró ShortView.");
        }
    }

    public void ReiniciarFeed()
    {
        if (feedSelector == null)
        {
            Debug.LogError("No se encontró FeedSelector.");
            return;
        }

        feedSelector.ReiniciarBiblioteca();

        CargarTutorial();

        Debug.Log(
            "Feed reiniciado → Tutorial cargado."
        );
    }
}