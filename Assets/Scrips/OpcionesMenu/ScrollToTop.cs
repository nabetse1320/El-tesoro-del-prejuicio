using UnityEngine;
using UnityEngine.UI;

public class ScrollToTop : MonoBehaviour
{
    private ScrollRect scrollRect;

    void Start()
    {
        // Aseg�rate de que el Scroll Rect est� en la posici�n inicial
        scrollRect = GetComponent<ScrollRect>();
        UpScroll();
    }
    public void UpScroll()
    {
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
