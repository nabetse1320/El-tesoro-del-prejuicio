using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenúInGame : MonoBehaviour
{
    public GameObject MenuPausa;
    public GameObject controlsMenu;
    public GameObject LogrosMenu;
    public GameObject[] elementosInGame;
    public GameObject[] elementosInMenu;
    public GameObject[] elementosMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!MenuPausa.activeSelf)
            {
                menuPausa();
            }
            else
            {
                Return();
            }
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            if(!LogrosMenu.activeSelf)
            {
                AbrirLogros();
            }
            else
            {
                Return();
            }
        }
    }
    public void menuPausa()
    {
        Time.timeScale = 0;
        foreach (GameObject elemento in elementosInGame)
        {
            if (elemento != null)
            {
                elemento.SetActive(false);
            }
        }
        LogrosMenu.SetActive(false);
        MenuPausa.SetActive(true);
        for (int i = 0; i < elementosInMenu.Length; i++)
        {
            if (i == 0)
            {
                elementosInMenu[i].SetActive(true);
            }
            else
            {
                elementosInMenu[i].SetActive(false);
            }
        }

    }
    public void AbrirLogros()
    {
        Time.timeScale = 0;
        foreach (GameObject elemento in elementosInGame)
        {
            if (elemento != null)
            {
                elemento.SetActive(false);
            }
        }
        foreach (GameObject elemento in elementosMenu)
        {
            if (elemento != null)
            {
                elemento.SetActive(false);
            }
        }
        LogrosMenu.SetActive(true);
        MenuPausa.SetActive(false);
    }


    public void controlsSettings()
    {
        elementosMenu[0].SetActive(true);
        elementosMenu[1].SetActive(false);
        elementosMenu[2].SetActive(false);
        for (int i = 0; i < elementosInMenu.Length; i++)
        {
            if(i == 3)
            {
                elementosInMenu[i].SetActive(true);
            }
            else
            {
                elementosInMenu[i].SetActive(false);
            }
        }
        
    }
    public void GeneralSettings()
    {
        elementosMenu[0].SetActive(false);
        elementosMenu[1].SetActive(true);
        elementosMenu[2].SetActive(false);
        for (int i = 0; i < elementosInMenu.Length; i++)
        {
            if (i == 1)
            {
                elementosInMenu[i].SetActive(true);
            }
            else
            {
                elementosInMenu[i].SetActive(false);
            }
        }
    }
    public void SoundSettings()
    {
        for (int i = 0; i < elementosInMenu.Length; i++)
        {
            if (i == 2)
            {
                elementosInMenu[i].SetActive(true);
            }
            else
            {
                elementosInMenu[i].SetActive(false);
            }
        }
        elementosMenu[0].SetActive(false);
        elementosMenu[1].SetActive(false);
        elementosMenu[2].SetActive(true);
    }
    public void Return()
    {
        Time.timeScale = 1;
        MenuPausa.SetActive(false);
        LogrosMenu.SetActive(false);
        foreach (GameObject elemento in elementosInGame)
        {
            if (elemento != null)
            {
                elemento.SetActive(true);
            }
        }
        foreach(GameObject elemento in elementosMenu)
        {
            if (elemento != null)
            {
                elemento.SetActive(false);
            }
        }
    }
     
     
    public void SalirAlMenu()
    {
        for (int i = 0; i < elementosInMenu.Length; i++)
        {
            if (i == 4)
            {
                elementosInMenu[i].SetActive(true);
            }
            else
            {
                elementosInMenu[i].SetActive(false);
            }
        }
        Time.timeScale = 1;
        Eventos.eve.PasarNivel?.Invoke(1);

    }
}
