using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Interruptor : MonoBehaviour
{

    [SerializeField] private int id;
    [SerializeField] private bool presionMantenida;
    private Animator animator;
    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioSource otherAudioSource;
    [SerializeField] private AudioClip audioActivar;
    [SerializeField] private AudioClip audioDesactivar;
    // Lo que esta encima del interruptor (jugador o cajas). Se apaga solo cuando no queda nada
    private readonly HashSet<Collider2D> encima = new HashSet<Collider2D>();
    private bool activado;


    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private static bool Cuenta(Collider2D c)
    {
        return c.CompareTag("Player") || c.CompareTag("ObInteract");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Cuenta(collision)) return;
        encima.Add(collision);
        if (!activado)
        {
            Activar();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Se quita aunque ya no tenga la etiqueta (al cambiar de personaje el anterior pasa a Untagged)
        if (encima.Remove(collision))
        {
            ComprobarVacio();
        }
    }

    private void FixedUpdate()
    {
        // Algo encima que se destruye o desactiva no llama a OnTriggerExit2D
        if (activado && presionMantenida && encima.Count > 0)
        {
            encima.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
            ComprobarVacio();
        }
    }

    private void ComprobarVacio()
    {
        if (activado && presionMantenida && encima.Count == 0)
        {
            Desactivar();
        }
    }

    private void Activar()
    {
        activado = true;
        if (otherAudioSource != null)
        {
            otherAudioSource.Play();
        }
        if (animator != null)
        {
            animator.SetBool("activado", true);
        }
        Eventos.eve.ActivarPlataforma?.Invoke(id);
        Eventos.eve.activarCuerda?.Invoke(id);
        if (audioSource != null)
        {
            audioSource.clip = audioActivar;
            audioSource.Play();
        }
    }

    private void Desactivar()
    {
        activado = false;
        Eventos.eve.DesactivarPlataforma.Invoke(id);
        Eventos.eve.DesactivarCuerda.Invoke(id);
        if (animator != null)
        {
            animator.SetBool("activado", false);
        }
        if (audioSource != null)
        {
            audioSource.clip = audioDesactivar;
            audioSource.Play();
        }
    }

}
