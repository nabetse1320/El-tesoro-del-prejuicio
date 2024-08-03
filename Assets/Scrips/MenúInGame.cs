using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;
using UnityEngine.SceneManagement;
public class MenúInGame : MonoBehaviour
{
    public GameObject menuDePausa;
    public GameObject menuOpciones;
    public GameObject controlsMenu;
    public GameObject LogrosMenu;
    public GameObject[] elementosInGame;
    public GameObject[] elementosInMenuOptions;
    public GameObject[] elementosMenuOpciones;
    [Header("RebindActionOpcions")]
    [SerializeField] private RebindSaveLoad rebindLoad;

    public void OnPauseStarted(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            if (!menuDePausa.activeSelf&&!menuOpciones.activeSelf)
            {
                menuPausa();
            }
            else if (menuOpciones.activeSelf)
            {
                ReturnToMenuPause();
            }else
            {
                //rebindLoad.ChargeRebinds();
                Return();

            }
        }
    }
    public void OnVocabularioStarted (InputAction.CallbackContext value)
    {
        if (value.started)
        {
            if (!LogrosMenu.activeSelf)
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
        foreach (GameObject element in elementosInGame)
        { 
            element.SetActive(false);
        }
        Eventos.eve.PausarPlayer.Invoke();
        Eventos.eve.PausarPlayer2.Invoke();
        LogrosMenu.SetActive(false);
        menuOpciones.SetActive(false);
        menuDePausa.SetActive(true);

    }
    public void AbrirMenuOpciones()
    {
        menuDePausa.SetActive(false);
        menuOpciones.SetActive(true);
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
        foreach (GameObject elemento in elementosMenuOpciones)
        {
            if (elemento != null)
            {
                elemento.SetActive(false);
            }
        }
        LogrosMenu.SetActive(true);
        menuDePausa.SetActive(false);
    }


    public void controlsSettings()
    {
        elementosMenuOpciones[0].SetActive(false);
        elementosMenuOpciones[1].SetActive(false);
        elementosMenuOpciones[2].SetActive(true);
        for (int i = 0; i < elementosInMenuOptions.Length; i++)
        {
            if(i == 2)
            {
                elementosInMenuOptions[i].SetActive(true);
            }
            else
            {
                elementosInMenuOptions[i].SetActive(false);
            }
        }
        
    }
    public void GeneralSettings()
    {
        elementosMenuOpciones[0].SetActive(true);
        elementosMenuOpciones[1].SetActive(false);
        elementosMenuOpciones[2].SetActive(false);
        for (int i = 0; i < elementosInMenuOptions.Length; i++)
        {
            if (i == 0)
            {
                elementosInMenuOptions[i].SetActive(true);
            }
            else
            {
                elementosInMenuOptions[i].SetActive(false);
            }
        }
    }
    public void SoundSettings()
    {
        for (int i = 0; i < elementosInMenuOptions.Length; i++)
        {
            if (i == 1)
            {
                elementosInMenuOptions[i].SetActive(true);
            }
            else
            {
                elementosInMenuOptions[i].SetActive(false);
            }
        }
        elementosMenuOpciones[0].SetActive(false);
        elementosMenuOpciones[1].SetActive(true);
        elementosMenuOpciones[2].SetActive(false);
    }
    public void Return()
    {
        Time.timeScale = 1;
        Eventos.eve.DespausarPlayer.Invoke();
        Eventos.eve.DespausarPlayer2.Invoke();
        menuDePausa.SetActive(false);
        LogrosMenu.SetActive(false);
        foreach (GameObject elemento in elementosInGame)
        {
            if (elemento != null)
            {
                elemento.SetActive(true);
            }
        }
        
    }
    public void ReturnToMenuPause()
    {
        for (int i = 0; i < elementosInMenuOptions.Length; i++)
        {
            elementosInMenuOptions[i].SetActive(false);
        }
        foreach (GameObject elemento in elementosMenuOpciones)
        {
            elemento.SetActive(false);
        }
        menuDePausa.SetActive(true);
        menuOpciones.SetActive(false);
        
        
    }
     
     
    public void SalirAlMenu()
    {
        Return();
        Eventos.eve.PasarNivel?.Invoke(1);

    }
}
