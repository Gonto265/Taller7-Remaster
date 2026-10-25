using UnityEngine;

public class InputManager : MonoBehaviour
{
    public System.Action SwipeUp;
    public System.Action<float> OnDrag;

    [Header("Configuración de Swipe y Toque")]
    [SerializeField] private float minimumSwipeDistance = 100f;
    [SerializeField] private float doubleTapThreshold = 0.3f; // Tiempo máximo para considerar un doble toque

    private Vector2 startPosition;
    private bool isSwiping;
    private float lastTapTime;

    private FeedManager feedManager;
    private IdleManager idleManager;
    private ShortInteraction shortInteraction;

    private void Awake()
    {
        feedManager = FindFirstObjectByType<FeedManager>();
        idleManager = FindFirstObjectByType<IdleManager>();
    
        SwipeUp += OnSwipeUp;
    }

    private void Update()
    {
        // 1. Verificamos si estamos en un estado donde se permite jugar
        if (!PuedeInteractuar())
        {
            if (isSwiping)
            {
                isSwiping = false;
                OnDrag?.Invoke(0f); 
            }
            return;
        }

        DetectMouseInput();
        DetectTouchInput();
    }

    private bool PuedeInteractuar()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return false;

        return gameManager.CurrentState == GameManager.GameState.Tutorial ||
               gameManager.CurrentState == GameManager.GameState.Playing;
    }

    // =========================================================
    // INPUT MOUSE / TOUCH
    // =========================================================

    private void DetectMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            isSwiping = true;

            // Detección de doble toque
            if (Time.time - lastTapTime < doubleTapThreshold)
            {
                EjecutarDobleToque();
            }
            lastTapTime = Time.time;
        }

        if (Input.GetMouseButton(0) && isSwiping)
        {
            float deltaY = Input.mousePosition.y - startPosition.y;
            OnDrag?.Invoke(deltaY);
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            Vector2 endPosition = Input.mousePosition;
            CheckSwipe(endPosition - startPosition);
            
            isSwiping = false;
            OnDrag?.Invoke(0f);
        }
    }

    private void DetectTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            startPosition = touch.position;
            isSwiping = true;

            // Detección de doble toque
            if (Time.time - lastTapTime < doubleTapThreshold)
            {
                EjecutarDobleToque();
            }
            lastTapTime = Time.time;
        }

        if (touch.phase == TouchPhase.Moved && isSwiping)
        {
            float deltaY = touch.position.y - startPosition.y;
            OnDrag?.Invoke(deltaY);
        }

        if (touch.phase == TouchPhase.Ended && isSwiping)
        {
            Vector2 endPosition = touch.position;
            CheckSwipe(endPosition - startPosition);
            
            isSwiping = false;
            OnDrag?.Invoke(0f);
        }
    }

    // =========================================================
    // ACCIONES
    // =========================================================

    private void EjecutarDobleToque()
    {
        lastTapTime = 0f; // Reiniciamos para evitar que 3 toques cuenten como 2 dobles toques

        if (shortInteraction == null)
        {
            shortInteraction = FindFirstObjectByType<ShortInteraction>();
        }

        if (shortInteraction != null)
        {
            if (!shortInteraction.Liked)
            {
                shortInteraction.Like();
            }
        }
    }

    private void CheckSwipe(Vector2 delta)
    {
        if (delta.y < minimumSwipeDistance) return;
        
        if (Mathf.Abs(delta.y) <= Mathf.Abs(delta.x)) return; 

        SwipeUp?.Invoke();
    }

    private void OnSwipeUp()
    {
        if (idleManager != null)
            idleManager.RegistrarInteraccion();

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null && gameManager.CurrentState == GameManager.GameState.Tutorial)
        {
            gameManager.IniciarJuego();
        }
        
        // El SwipeAnimator se encarga de cambiar el video, por eso FeedManager.Next() no está aquí.
    }
}