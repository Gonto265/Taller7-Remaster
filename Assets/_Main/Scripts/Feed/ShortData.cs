using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(
    fileName = "Short_",
    menuName = "Taller7/Short Data"
)]
public class ShortData : ScriptableObject
{
    [Header("Identificación")]
    public string id;

    [Header("Contenido")]
    public VideoClip video;
    public string titulo;
    public string fuente;

    [Header("Clasificación")]
    [Range(1, 3)]
    public int sensacionalismo;

    [Range(1, 3)]
    public int falsedad;

    public string tematica;

    [Header("Búsqueda")]
    public SearchOption[] searchOptions;
}