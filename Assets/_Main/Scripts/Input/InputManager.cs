using UnityEngine;

public class InputManager : MonoBehaviour
{
    public System.Action SwipeUp;
    public System.Action<float> OnDrag; // NUEVO: Avisa cuánto se ha movido el dedo en Y

    [SerializeField] private float minimumSwipeDistance = 100f;

    private Vector2 startPosition;
    private bool isSwiping;

    private FeedManager feedManager;

    private IdleManager idleManager;

    private void Awake()
    {
        feedManager = FindFirstObjectByType<FeedManager>();
        idleManager = FindFirstObjectByType<IdleManager>();
    
        SwipeUp += OnSwipeUp;
    }

    private void Update()
    {
        DetectMouseInput();
        DetectTouchInput();
    }

    private void DetectMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            isSwiping = true;
        }

        // NUEVO: Detectar el movimiento mientras se mantiene presionado
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

            // NUEVO: Avisamos que el arrastre terminó enviando un 0
            OnDrag?.Invoke(0f);
        }
    }

    private void DetectTouchInput()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            startPosition = touch.position;
            isSwiping = true;
        }

        // NUEVO: Detectar el movimiento del dedo en la pantalla
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

            // NUEVO: Avisamos que el arrastre terminó
            OnDrag?.Invoke(0f);
        }
    }

    private void CheckSwipe(Vector2 delta)
    {
        if (delta.y < minimumSwipeDistance)
            return;

        if (Mathf.Abs(delta.y) <= Mathf.Abs(delta.x))
            return;

        SwipeUp?.Invoke();
    }

    private void OnSwipeUp()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
    
        if (gameManager == null)
            return;
    
        // Solo se puede hacer swipe durante Tutorial o Playing
        if (gameManager.CurrentState != GameManager.GameState.Tutorial &&
            gameManager.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }
    
        if (idleManager != null)
            idleManager.RegistrarInteraccion();
    
        if (gameManager.CurrentState == GameManager.GameState.Tutorial)
        {
            gameManager.IniciarJuego();
        }
    
        
    }
}