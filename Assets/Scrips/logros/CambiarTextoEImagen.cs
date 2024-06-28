using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CambiarTextoEImagen : MonoBehaviour
{
    public VerticalLayoutGroup verticalLayoutGroup;
    // Start is called before the first frame update
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private UnityEngine.UI.Image imagen;
    [Header("Texto a cambiar")]
    [SerializeField][TextArea(10, 30)]private string nuevoTexto;
    [SerializeField] private Sprite newImage;
    
    public void CambiarElTextoEImagen()
    {
        if (!imagen.gameObject.activeSelf)
        {
            imagen.gameObject.SetActive(true);
        }
        imagen.sprite = newImage;
        texto.text = nuevoTexto;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)verticalLayoutGroup.transform);
    }
}
