using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logros : MonoBehaviour
{
    //public static int logrosYaDesbloqueados;
    [SerializeField] private GameObject[] logrosBloqueados;
    [SerializeField] private GameObject[] logrosDesbloqueados;
    [SerializeField] private Color colorDeSombreadoDeBotones = Color.gray;

    private void Update()
    {
        DesbloquearLosLogros();
    }
    public void DesbloquearLosLogros()
    {
        for (int i = 0; i < logrosDesbloqueados.Length; i++)
        {
            if (PlayerPrefs.GetInt("Logro"+ i, 0) != 0)
            {
                DesbloquearLogro(i);
            }
        }
    }
    public void ResetearLogros(int tamanoVectorLogros)
    {
        CambiarLogrosDesbloqueados.ReiniciarLogros(tamanoVectorLogros);
    }
    public void DesbloquearLogro(int logro)
    {
        logrosBloqueados[logro].SetActive(false);
        logrosDesbloqueados[logro].SetActive(true);
    }

    public void SombreadoBotones()
    {
        foreach(GameObject logro in logrosDesbloqueados)
        {
            if (logro.GetComponent<ButtonHighlightController>().isHighlighted)
            {
                logro.GetComponent<Button>().image.color = colorDeSombreadoDeBotones;
                logro.GetComponent<ButtonHighlightController>().isHighlighted = false;
            }
            else
            {
                logro.GetComponent<Button>().image.color = logro.GetComponent<ButtonHighlightController>().originalColor;  
            }
            
        }
    }
}
