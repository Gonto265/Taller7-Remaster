using System.Collections.Generic;
using UnityEngine;

public class FeedSelector : MonoBehaviour
{
    [SerializeField] private ShortData[] shorts;

    private List<ShortData> shortsDisponibles = new List<ShortData>();
    private List<ShortData> shortsUtilizados = new List<ShortData>();

    private DoomingManager doomingManager;

    private void Awake()
    {
        doomingManager = FindFirstObjectByType<DoomingManager>();

        ReiniciarBiblioteca();
    }

    public void ReiniciarBiblioteca()
    {
        shortsDisponibles.Clear();
        shortsUtilizados.Clear();

        if (shorts == null)
            return;

        foreach (ShortData shortData in shorts)
        {
            if (shortData != null)
            {
                shortsDisponibles.Add(shortData);
            }
        }
    }

    public ShortData ObtenerSiguiente()
    {
        if (shortsDisponibles.Count == 0)
        {
            Debug.Log("Biblioteca completada. Reiniciando Shorts disponibles.");
    
            ReiniciarBiblioteca();
        }
    
        List<ShortData> candidatos =
            BuscarCandidatosSegunDooming();
    
        if (candidatos.Count == 0)
        {
            Debug.Log("No se encontraron candidatos.");
            return null;
        }
    
        int indice = Random.Range(0, candidatos.Count);
    
        ShortData seleccionado = candidatos[indice];
    
        shortsDisponibles.Remove(seleccionado);
        shortsUtilizados.Add(seleccionado);
    
        Debug.Log(
            $"Short seleccionado: {seleccionado.name} " +
            $"(S{seleccionado.sensacionalismo}/F{seleccionado.falsedad})"
        );
    
        return seleccionado;
    }

    public List<ShortData> ObtenerCandidatos(int nivelSensacionalismo, int nivelFalsedad)
    {
        List<ShortData> candidatos = new List<ShortData>();
    
        foreach (ShortData shortData in shortsDisponibles)
        {
            if (shortData.sensacionalismo == nivelSensacionalismo &&
                shortData.falsedad == nivelFalsedad)
            {
                candidatos.Add(shortData);
            }
        }
    
        return candidatos;
    }

    

    public List<ShortData> ObtenerCandidatosCercanos(
    int nivelSensacionalismo,
    int nivelFalsedad)
    {
        List<ShortData> candidatos = new List<ShortData>();
    
        int menorDistancia = int.MaxValue;
    
        foreach (ShortData shortData in shortsDisponibles)
        {
            int distancia =
                Mathf.Abs(
                    shortData.sensacionalismo - nivelSensacionalismo
                )
                +
                Mathf.Abs(
                    shortData.falsedad - nivelFalsedad
                );
    
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
    
                candidatos.Clear();
                candidatos.Add(shortData);
            }
            else if (distancia == menorDistancia)
            {
                candidatos.Add(shortData);
            }
        }
    
        return candidatos;
    }

    public List<ShortData> BuscarCandidatos(
    int nivelSensacionalismo,
    int nivelFalsedad)
    {
        List<ShortData> candidatosExactos =
            ObtenerCandidatos(
                nivelSensacionalismo,
                nivelFalsedad
            );
    
        if (candidatosExactos.Count > 0)
        {
            return candidatosExactos;
        }
    
        return ObtenerCandidatosCercanos(
            nivelSensacionalismo,
            nivelFalsedad
        );
    }

    public List<ShortData> BuscarCandidatosSegunDooming()
    {
        if (doomingManager == null)
        {
            Debug.LogError("No se encontró DoomingManager.");
            return new List<ShortData>();
        }
    
        int nivelS = doomingManager.ObtenerNivel(
            doomingManager.Sensacionalismo
        );
    
        int nivelF = doomingManager.ObtenerNivel(
            doomingManager.Falsedad
        );
    
        Debug.Log(
            $"Buscando Shorts para Dooming S{nivelS}/F{nivelF}"
        );
    
        return BuscarCandidatos(nivelS, nivelF);
    }

    public bool YaFueUtilizado(ShortData shortData)
    {
        return shortsUtilizados.Contains(shortData);
    }

    public int ObtenerCantidadDisponibles()
    {
        return shortsDisponibles.Count;
    }

    public int ObtenerCantidadUtilizados()
    {
        return shortsUtilizados.Count;
    }

    public void MostrarEstadoDooming()
    {
        if (doomingManager == null)
        {
            Debug.LogError("No se encontró DoomingManager.");
            return;
        }
    
        int nivelS = doomingManager.ObtenerNivel(
            doomingManager.Sensacionalismo
        );
    
        int nivelF = doomingManager.ObtenerNivel(
            doomingManager.Falsedad
        );
    
        Debug.Log(
            $"Estado Dooming → Sensacionalismo: {nivelS} | Falsedad: {nivelF}"
        );
    }
}