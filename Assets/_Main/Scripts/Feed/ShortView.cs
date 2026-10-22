using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

public class ShortView : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    [Header("Interacción")]
    [SerializeField] private ShortInteraction shortInteraction;

    [Header("Panel de búsqueda")]
    [SerializeField] private GameObject searchPanel;
    [SerializeField] private Button[] searchButtons;

    [Header("Información del Short")]
    [SerializeField] private TMP_Text tematicaText;
    [SerializeField] private TMP_Text tituloText;
    [SerializeField] private TMP_Text fuenteText;

    [Header("Renderizado")]
    [SerializeField] private RenderTexture videoRenderTexture;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
        {
            Debug.LogError("ShortView necesita un VideoPlayer.");
        }

        if (shortInteraction == null)
        {
            Debug.LogError("ShortView necesita una referencia a ShortInteraction.");
        }
    }

    public void MostrarShort(ShortData shortData)
    {
        if (shortData == null)
        {
            Debug.LogWarning("ShortView recibió un ShortData vacío.");
            return;
        }

        if (tematicaText != null)
            tematicaText.text = shortData.tematica;
        
        if (tituloText != null)
            tituloText.text = shortData.titulo;

        if (videoPlayer == null)
            return;

        if (shortData.video == null)
        {
            Debug.LogWarning(
                $"El Short {shortData.name} no tiene VideoClip asignado."
            );
            return;
        }

        videoPlayer.Stop();

        // 1. Limpiamos la memoria del video anterior para que quede en negro
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
        }

        videoPlayer.clip = shortData.video;
        videoPlayer.isLooping = true;

        // 2. En lugar de darle a Play de inmediato, preparamos el video
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        ConfigurarBusqueda(shortData);

        Debug.Log(
            $"Reproduciendo video de: {shortData.name}"
        );
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // Muy importante desuscribirse para no acumular llamadas en futuros swipes
        vp.prepareCompleted -= OnVideoPrepared;
        
        vp.Play();
    }

    public void AbrirBusqueda()
    {
        if (searchPanel != null)
        {
            searchPanel.SetActive(true);
        }
    }

    private void ConfigurarBusqueda(ShortData shortData)
    {
        if (searchPanel == null)
            return;
    
        searchPanel.SetActive(false);
    
        if (searchButtons == null || searchButtons.Length == 0)
        {
            return;
        }
    
        if (shortData.searchOptions == null ||
            shortData.searchOptions.Length == 0)
        {
            return;
        }
    
        for (int i = 0; i < searchButtons.Length; i++)
        {
            Button boton = searchButtons[i];
    
            if (boton == null)
                continue;
    
            boton.onClick.RemoveAllListeners();
    
            if (i >= shortData.searchOptions.Length)
            {
                boton.gameObject.SetActive(false);
                continue;
            }
    
            boton.gameObject.SetActive(true);
    
            SearchOption opcion = shortData.searchOptions[i];
    
            TMP_Text texto = boton.GetComponentInChildren<TMP_Text>();
    
            if (texto != null)
            {
                texto.text = opcion.texto;
            }
    
            int indice = i;
    
            boton.onClick.AddListener(() =>
            {
                shortInteraction.Search(indice);
                searchPanel.SetActive(false);
            });
        }
    }

    public void DetenerVideo()
    {
        if (videoPlayer == null)
            return;
    
        videoPlayer.Stop();
    
        Debug.Log("Video detenido porque la partida terminó.");
    }
}