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
    private bool primerTriggerYaEntró = false;
    private Collider2D primerObjeto;
    private String tagPO;
   

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("ObInteract"))
        {
            if (!primerTriggerYaEntró)
            {
                if (otherAudioSource != null)
                {
                    otherAudioSource.Play();
                }
                primerTriggerYaEntró = true;
                animator.SetBool("activado",true);
                primerObjeto = collision;
                tagPO = collision.tag;
                Eventos.eve.ActivarPlataforma?.Invoke(id);
                Eventos.eve.activarCuerda?.Invoke(id);
                audioSource.clip = audioActivar;
                audioSource.Play();
                // Aquí va tu código para cuando el primer trigger entra al collider
            }
        }
        
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.CompareTag("Player") || collision.CompareTag("ObInteract")) && presionMantenida)
        {
            if (primerObjeto == collision)
            {
                primerTriggerYaEntró = false;
                Eventos.eve.DesactivarPlataforma.Invoke(id);
                Eventos.eve.DesactivarCuerda.Invoke(id);
                audioSource.clip = audioDesactivar;
                animator.SetBool("activado", false);
                audioSource.Play();

            }
            else if (tagPO==("Player") && collision.CompareTag("Player"))
            {
                primerTriggerYaEntró = false;
                Eventos.eve.DesactivarPlataforma.Invoke(id);
                Eventos.eve.DesactivarCuerda.Invoke(id);
                audioSource.clip = audioDesactivar;
                animator.SetBool("activado", false);
                audioSource.Play();
            }
        }
        
        
    }

}
