using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHighlightController : MonoBehaviour, IPointerDownHandler
{
    private Button button;
    [HideInInspector] public Color originalColor;
    [HideInInspector] public bool isHighlighted = false;

    private void Start()
    {
        button = GetComponent<Button>();
        originalColor = button.colors.normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHighlighted = true;
    }

}
