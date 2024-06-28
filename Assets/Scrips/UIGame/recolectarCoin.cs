using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class recolectarCoin : MonoBehaviour
{
    [SerializeField] private AnimationClip clipRecolect;
    [SerializeField] private int logro=0;
    private Animator animator;
    private Collider2D collider;
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collider.enabled = false;
            Eventos.eve.coinsCount.Invoke(1);
            CambiarLogrosDesbloqueados.CambiarLogrosBloqueados(logro);
            animator.SetBool("Recolectado", true);
            if (audioSource != null)
            {
                audioSource.Play();
            }
            Destroy(gameObject,clipRecolect.length);
        }
    }
}
