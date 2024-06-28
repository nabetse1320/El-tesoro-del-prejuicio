using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolDownBarra : MonoBehaviour
{
    // Start is called before the first frame update
    private Slider slider;
    void Start()
    {
        slider = GetComponent<Slider>();
        slider.maxValue = 1;
        slider.value = slider.maxValue;
    }

    private void CambiarValor(float valor)
    {
        slider.value = valor;
    }
    private void OnEnable()
    {
        Eventos.eve.cambiarBarraCoolDown.AddListener(CambiarValor);
    }
    private void OnDisable()
    {
        Eventos.eve.cambiarBarraCoolDown.RemoveListener(CambiarValor);
    }
}
