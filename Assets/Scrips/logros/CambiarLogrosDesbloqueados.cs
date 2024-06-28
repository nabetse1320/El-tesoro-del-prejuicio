using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CambiarLogrosDesbloqueados : MonoBehaviour
{
    public static void ReiniciarLogros(int logrosLenght)
    {
        for (int i = 0; i < logrosLenght; i++)
        {
            PlayerPrefs.SetInt("Logro" + i, 0);
        }
    }
    public static void CambiarLogrosBloqueados(int num)
    {
        PlayerPrefs.SetInt("Logro"+num, 1);
    }
}
