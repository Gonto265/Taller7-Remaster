using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Pillarbox : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Forzamos la proporción estricta de celular (9:16)
        float targetAspect = 9.0f / 16.0f; 
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        // Si la pantalla es más ancha que un celular (Ej. Monitor de PC) -> Barras a los lados
        if (scaleHeight >= 1.0f)
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
        // Si la pantalla es más alta que un celular -> Barras arriba y abajo
        else
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
    }
}