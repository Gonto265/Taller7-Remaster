using UnityEngine;

public class CloseSearchPanel : MonoBehaviour
{
    [SerializeField] private GameObject searchPanel;

    public void Cerrar()
    {
        if (searchPanel != null)
        {
            searchPanel.SetActive(false);
        }
    }
}