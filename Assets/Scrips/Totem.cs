using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Totem : MonoBehaviour
{
    [SerializeField] private int daño=5;
    [SerializeField] private bool reusable = true;
    [Header("Si no es reusable:")]
    [SerializeField] private int usos = 1;
    [Tooltip("Segundos hasta que el totem puede volver a golpear (si le quedan usos)")]
    [SerializeField] private float tiempoRecarga = 1f;
    private Collider2D collider;
    Animator animator;
    [Header("Sounds fx")]
    private AudioSource audioSource;
    private bool gastado;

    private void Start()
    {
        collider= GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gastado || !collision.CompareTag("Enemy")) return;

        Fantasma fantasma = collision.GetComponent<Fantasma>();
        if (fantasma == null) return;

        // Se apaga un momento para no golpear varias veces al mismo fantasma
        collider.enabled = false;
        fantasma.ResivirDaño(daño);
        if (!reusable)
        {
            usos -= 1;
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }

        if (!reusable && usos <= 0)
        {
            // Gastado: animacion de destruccion y se elimina una sola vez (antes se pedia en cada frame)
            gastado = true;
            if (animator != null) animator.SetBool("usado", true);
            float espera = audioSource != null && audioSource.clip != null ? audioSource.clip.length - 2f : 1f;
            Destroy(gameObject, Mathf.Max(0.5f, espera));
        }
        else
        {
            // Reutilizable o con usos restantes: vuelve a funcionar tras la recarga
            // (antes el collider se quedaba apagado para siempre)
            Invoke(nameof(Recargar), tiempoRecarga);
        }
    }

    private void Recargar()
    {
        if (!gastado && collider != null) collider.enabled = true;
    }
}
