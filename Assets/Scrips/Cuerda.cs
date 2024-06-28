using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuerda : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float tiempoEspera=1;
    [SerializeField] private bool aparecer;
    [SerializeField] private int id;
    [SerializeField] private GameObject child;
    [Header("Multinterruptores")]
    [SerializeField] private bool multiInterruptor = false;
    [SerializeField] private int numeroInterruptores = 2;
    [Header("SoundEfects")]
    [SerializeField] private AudioSource AudioSource;
    private int cont;
    private bool act;
    private bool desact;
    private void Start()
    {
        act = true;
        desact = false;
        if(AudioSource != null)
        {
            AudioSource = GetComponent<AudioSource>();
        }
        if (aparecer)
        {
            child.SetActive(false);
            act = false;
            desact = true;
        }
        cont = 0;
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
    public void Desactivar()
    {
        act = false;
        desact = true;
        StartCoroutine(Espera());
    }
    public void Activar()
    {
        if (AudioSource != null)
        {
            AudioSource.Play();
        }
        act = true;
        desact = false;
        StartCoroutine(EsperaAct());
    }
    IEnumerator Espera()
    {
        yield return new WaitForSeconds(tiempoEspera);
        if (child != null)
        {
            child.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
    IEnumerator EsperaAct()
    {
        yield return new WaitForSeconds(tiempoEspera);
        if (child!=null)
        {
            child.SetActive(true);
        }
    }
    private void ActivarPlataforma(int idResiver)
    {
        if (idResiver == id)
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
                Activar();
            }
            
        }

    }
    private void DesactivarPlataforma(int idResiver)
    {
        if (idResiver == id)
        {
            if (multiInterruptor)
            {
                cont--;
            }
            else
            {
                Desactivar();
            }
            
        }

    }

    private void OnEnable()
    {
        Eventos.eve.activarCuerda.AddListener(ActivarPlataforma);
        Eventos.eve.DesactivarCuerda.AddListener(DesactivarPlataforma);
    }
    private void OnDisable()
    {
        Eventos.eve.activarCuerda.RemoveListener(ActivarPlataforma);
        Eventos.eve.DesactivarCuerda.RemoveListener(DesactivarPlataforma);
    }
}
