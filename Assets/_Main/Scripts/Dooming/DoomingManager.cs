using UnityEngine;

public class DoomingManager : MonoBehaviour
{
    [Header("Valores actuales")]
    [SerializeField] private float sensacionalismo = 1f;
    [SerializeField] private float falsedad = 1f;

    [Header("Progresión automática")]
    [SerializeField] private float aumentoPorSegundo = 0.30f;

    private GameManager gameManager;

    public float Sensacionalismo => sensacionalismo;
    public float Falsedad => falsedad;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (gameManager == null)
            return;

        if (gameManager.CurrentState != GameManager.GameState.Playing)
            return;

        sensacionalismo += aumentoPorSegundo * Time.deltaTime;
        falsedad += aumentoPorSegundo * Time.deltaTime;

        if (HayDoomingCritico())
        {
            gameManager.FinalizarPorCritico();
        }
    }

    public void ModificarSensacionalismo(float cantidad)
    {
        sensacionalismo = Mathf.Max(
            1f,
            sensacionalismo + cantidad
        );
    }

    public void ModificarFalsedad(float cantidad)
    {
        falsedad = Mathf.Max(
            1f,
            falsedad + cantidad
        );
    }

    public Vector2 ModificarDooming(
        float cambioSensacionalismo,
        float cambioFalsedad)
    {
        float valorAnteriorS = sensacionalismo;
        float valorAnteriorF = falsedad;

        sensacionalismo = Mathf.Max(
            1f,
            sensacionalismo + cambioSensacionalismo
        );

        falsedad = Mathf.Max(
            1f,
            falsedad + cambioFalsedad
        );

        float cambioRealS =
            sensacionalismo - valorAnteriorS;

        float cambioRealF =
            falsedad - valorAnteriorF;

        return new Vector2(
            cambioRealS,
            cambioRealF
        );
    }

    public int ObtenerNivel(float valor)
    {
        if (valor <= 30f)
            return 1;

        if (valor <= 60f)
            return 2;

        if (valor <= 90f)
            return 3;

        return 4;
    }

    public bool HayDoomingCritico()
    {
        return sensacionalismo > 90f ||
               falsedad > 90f;
    }

    public void ReiniciarDooming()
    {
        sensacionalismo = 1f;
        falsedad = 1f;
    
        Debug.Log("Dooming reiniciado → S1/F1.");
    }
}