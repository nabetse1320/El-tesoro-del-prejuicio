using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.Video;
using UnityEngine.Windows;

public class CambiarCinematica : MonoBehaviour
{
    private Mapa inputs;
    [SerializeField] private VideoPlayer clip;
    [SerializeField] private int sceneName;
    private void Awake()
    {
        inputs = new Mapa();
        inputs.Enable();
        StartCoroutine(FinalizarCinematica());
    }

    private void OnDestroy()
    {
        // Liberar las acciones de entrada creadas en Awake (evita fugas al cambiar de escena)
        inputs?.Disable();
        inputs?.Dispose();
    }
    IEnumerator FinalizarCinematica()
    {
        yield return new WaitForSeconds((float)clip.length);
        Eventos.eve.PasarNivel?.Invoke(sceneName);
    }

    private void OnPresPerformed(InputAction.CallbackContext value)
    {
        Eventos.eve.PasarNivel?.Invoke(sceneName);
    }
    private void OnEnable()
    {
        inputs.Player.Atacar.started += OnPresPerformed;
    }
    private void OnDisable()
    {
        inputs.Player.Atacar.started -= OnPresPerformed;
    }
}

