using UnityEngine;
using UnityEngine.UI;

public class ScrollToTop : MonoBehaviour
{
    private ScrollRect scrollRect;

    void Start()
    {
        // Asegúrate de que el Scroll Rect esté en la posición inicial
        scrollRect = GetComponent<ScrollRect>();
        UpScroll();
    }
    public void UpScroll()
    {
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
