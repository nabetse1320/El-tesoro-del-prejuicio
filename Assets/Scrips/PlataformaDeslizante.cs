using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlataformaDeslizante : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Multinterruptores")]
    [SerializeField] private bool multiInterruptor=false;
    [SerializeField] private int numeroInterruptores=2;

    [Header("Generales")]
    [SerializeField] private bool iniciaActivado = false;
    [SerializeField] private int idPlataforma;
    [Header("Id Modificable")]
    [SerializeField] private bool idModificable = false;
    [SerializeField] private int maxId=2;
    private Animator animator;
    [Header("SoundFX")]
    private AudioSource audioSource;
    private int cont;
    private bool act;
    private bool desact;


    private void Start()
    {
        act = false;
        desact = true;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        cont=0;
        if (iniciaActivado)
        {
            animator.SetBool("deslizar", true);
            act = true;
            desact = false;
        }
    }
    private void Update()
    {
        if (multiInterruptor)
        {
            if (cont == numeroInterruptores)
            {
                if (!act)
                {
                    Activar();
                }
            }
            else 
            {
                if (!desact)
                {
                    Desactivar();
                }
            }
        }
        
    }

    private void ActivarPlataforma(int idResiver)
    {
        if (idResiver == idPlataforma)
        {
            if (multiInterruptor)
            {
                if (cont != numeroInterruptores)
                {
                    cont++;
                }
            }
            else
            {
                if (animator.GetBool("deslizar"))
                {
                    Activar();
                }
                else
                {
                    Desactivar();
                }
            }
        }      
    }
    private void Activar()
    {
        animator.SetBool("deslizar", false);
        audioSource.Play();
        if (idModificable)
        {
            if (idPlataforma<maxId)
            {
                idPlataforma++;
            }
        }
        act = true;
        desact = false;
    }
    private void Desactivar()
    {
        animator.SetBool("deslizar", true);
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        if (idModificable)
        {
            if (idPlataforma < maxId)
            {
                idPlataforma++;
            }
        }
        act = false;
        desact = true;
    }
    private void DesactivarPlataforma(int idResiver)
    {
        if (idResiver==idPlataforma)
        {
            if (multiInterruptor)
            {
                cont--;
            }
            else
            {
                if (animator.GetBool("deslizar"))
                {
                    Activar();
                }
                else
                {
                    Desactivar();
                }
            }
        }
    }

    private void OnEnable()
    {
        Eventos.eve.ActivarPlataforma.AddListener(ActivarPlataforma);
        Eventos.eve.DesactivarPlataforma.AddListener(DesactivarPlataforma);
    }
    private void OnDisable()
    {
        Eventos.eve.ActivarPlataforma.RemoveListener(ActivarPlataforma);
        Eventos.eve.DesactivarPlataforma.RemoveListener(DesactivarPlataforma);
    }
}
