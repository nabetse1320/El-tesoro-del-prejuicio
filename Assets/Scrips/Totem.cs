using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Totem : MonoBehaviour
{
    [SerializeField] private int daño=5;
    [SerializeField] private bool reusable = true;
    [Header("Si no es reusable:")]
    [SerializeField] private int usos = 1;
    private Collider2D collider;
    Animator animator;
    [Header("Sounds fx")]
    private AudioSource audioSource;
    private void Start()
    {
        collider= GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (!reusable)
        {
            if (usos <= 0)
            {
                animator.SetBool("usado", true);
                Destroy(gameObject,audioSource.clip.length-2f);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {

            if (collision.GetComponent<Fantasma>() != null)
            {
                collider.enabled = false;
                collision.GetComponent<Fantasma>().ResivirDaño(daño);
                if (!reusable)
                {
                    usos -= 1;
                    if (audioSource != null)
                    {
                        audioSource.Play();
                    }
                }
            }
        }
    }
}
