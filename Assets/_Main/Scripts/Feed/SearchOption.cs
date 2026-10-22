using System;
using UnityEngine;

[Serializable]
public class SearchOption
{
    public enum SearchType
    {
        Negativa,
        Neutral,
        Positiva
    }

    [TextArea(2, 4)]
    public string texto;

    public SearchType tipo;

    public float ObtenerEfectoFalsedad()
    {
        switch (tipo)
        {
            case SearchType.Negativa:
                return 5f;

            case SearchType.Neutral:
                return 0f;

            case SearchType.Positiva:
                return -10f;

            default:
                return 0f;
        }
    }
}