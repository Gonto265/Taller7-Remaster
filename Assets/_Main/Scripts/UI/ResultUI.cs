using UnityEngine;
using TMPro;

public class ResultUI : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject doomPanel;

    [Header("Resultado")]
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text resultDescription;

    [Header("Doom Panel Textos")]
    [SerializeField] private TMP_Text doomTitleText;
    [SerializeField] private TMP_Text doomDescriptionText;

    private void Start()
    {
        OcultarTodo();
    }

    public void MostrarResultado(GameManager.ResultType resultado)
    {
        OcultarTodo();

        if (resultPanel == null)
            return;

        resultPanel.SetActive(true);

        switch (resultado)
        {
            case GameManager.ResultType.FeedPerfecto:

                resultTitle.text = "FEED PERFECTO";
                resultDescription.text =
                    "Mantuviste el feed en un estado equilibrado.";

                break;

            case GameManager.ResultType.Neutral:

                resultTitle.text = "ESTADO NEUTRAL";
                resultDescription.text =
                    "La partida terminó sin alcanzar un estado crítico.";

                break;

            default:

                resultTitle.text = "RESULTADO";
                resultDescription.text =
                    "La partida ha terminado.";

                break;
        }
    }

    public void MostrarDoom(bool porScroll = false)
    {
        OcultarTodo();
        if (doomPanel != null) doomPanel.SetActive(true);

        if (porScroll)
        {
            if (doomTitleText != null) doomTitleText.text = "ATENCIÓN CONSUMIDA";
            if (doomDescriptionText != null) doomDescriptionText.text = "Deslizas sin prestar atención. Tu concentración ha decaído y el contenido tóxico consumió tu feed por completo.";
        }
        else
        {
            if (doomTitleText != null) doomTitleText.text = "ESTADO CRÍTICO";
            if (doomDescriptionText != null) doomDescriptionText.text = "El feed ha colapsado por exceso de sensacionalismo y falsedad.";
        }
    }

    private void OcultarTodo()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (doomPanel != null)
            doomPanel.SetActive(false);
    }

    public void OcultarResultados()
    {
        OcultarTodo();
    }

    public void MostrarFinalNoScrollear()
    {
        OcultarTodo();
    
        if (resultPanel == null)
            return;
    
        resultPanel.SetActive(true);
    
        resultTitle.text =
            "ERES DE LAS POCAS PERSONAS";
    
        resultDescription.text =
            "No te interesa scrollear. " +
            "Decidiste quedarte sin interactuar con el feed.";
    }
}