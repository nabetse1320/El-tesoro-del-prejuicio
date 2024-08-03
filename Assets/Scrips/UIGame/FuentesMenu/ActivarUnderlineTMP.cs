using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActivarUnderlineTMP : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI text;

    public void ActivarUnderline()
    {
        text.fontStyle = FontStyles.Underline;
    }
    public void DesactivarUnderline()
    {
        text.fontStyle &= ~FontStyles.Underline;
    }
}
