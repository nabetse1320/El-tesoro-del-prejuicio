using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fantasma : MonoBehaviour
{
    [SerializeField] public int daño;
    [SerializeField] private int vida = 30;
    [SerializeField] private float speed = 1;
    [SerializeField] private float detectionRange = 10.0f;
    [SerializeField] private float chargeDistance = 5.0f;
    [SerializeField] private float stunTime = 1;
    private Transform player;
    private Vector3 targetPosition;
    [SerializeField] private bool isCharging = false;
    [SerializeField] private bool resibiendoDano;
    private Collider2D myCollider;
    [SerializeField]private bool isReturning = false;
    private bool detener;
    private Animator animator;
    Vector3 playerPositionAtack;
    [SerializeField] private GameObject objetoDelCollider;
    private Collider2D collider;
    private bool muerto;
    private bool resibDano;

    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private AudioClip clipResivirDano;
    [SerializeField] private AudioClip clipMuerte;

    void Start()
    {
        collider = GetComponent<Collider2D>();
        muerto = false;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        myCollider = objetoDelCollider.GetComponent<Collider2D>();
        resibiendoDano = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        detener = false;
    }

    void Update()
    {
        if (!muerto)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            float angulo = 0;
            if (directionToPlayer.x > 0)
            {
                angulo = 180; // Rota hacia la derecha
            }
            else if (directionToPlayer.x < 0)
            {
                angulo = 0; // Rota hacia la izquierda
            }
            transform.rotation = Quaternion.AngleAxis(angulo, Vector3.up);
            if (!resibiendoDano)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                // Si el jugador está dentro del rango de detección y el enemigo no está cargando ni volviendo
                if (distanceToPlayer <= detectionRange && !isCharging && !isReturning && myCollider.bounds.Contains(player.position))
                {
                    // Establece la posición objetivo en la dirección del jugador y marca al enemigo como cargando
                    targetPosition = player.position + (player.position - transform.position).normalized * chargeDistance;
                    isCharging = true;
                    animator.SetBool("Atacando", true);
                    audioSource.clip = clipAtaque;
                    audioSource.Play();
                }

                // Si el enemigo ha alcanzado la posición objetivo, deja de cargar
                if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
                {
                    isCharging = false;
                    isReturning = false;
                    animator.SetBool("Atacando", false);

                }

                // Mueve al enemigo hacia la posición objetivo
                if (isCharging)
                {
                    Vector3 nextPosition = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);

                    if (myCollider.bounds.Contains(nextPosition))
                    {
                        transform.position = nextPosition;

                    }
                    else
                    {
                        //animator.SetBool("Atacando", false);
                        if (!detener)
                        {
                            StartCoroutine(espera());
                        }
                    }
                }
                // Si el enemigo no está cargando y está fuera del Collider, muévelo hacia el borde del Collider
                else if (!myCollider.bounds.Contains(transform.position))
                {
                    Vector3 directionToCenter = (myCollider.bounds.center - transform.position).normalized;
                    Vector3 pointOnEdge = myCollider.bounds.center - directionToCenter * myCollider.bounds.extents.magnitude;
                    transform.position = Vector3.Lerp(transform.position, pointOnEdge, speed * Time.deltaTime);
                    isReturning = true;

                }
            }
            else
            {
                if (!resibDano)
                {
                    audioSource.clip = clipResivirDano;
                    audioSource.Play();
                    resibDano = true;
                }
                Vector3 oppositeDirection = transform.position - player.position;

                if (!isCharging && !isReturning)
                {
                    targetPosition = transform.position + oppositeDirection.normalized * (chargeDistance);
                    isCharging = true;
                }
                if (Vector3.Distance(transform.position, targetPosition) <= 1f)
                {
                    isCharging = false;
                    resibiendoDano = false;
                    resibDano = false;
                    isReturning = false;
                }
                if (isCharging)
                {
                    Vector3 nextPosition = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
                    if (myCollider.bounds.Contains(nextPosition))
                    {
                        transform.position = nextPosition;
                    }
                    else
                    {
                        resibiendoDano = false;
                        if (!detener)
                        {
                            StartCoroutine(espera());
                        }
                    }
                }
                // Si el enemigo no está cargando y está fuera del Collider, muévelo hacia el borde del Collider
                else if (!myCollider.bounds.Contains(transform.position))
                {
                    Vector3 directionToCenter = (myCollider.bounds.center - transform.position).normalized;
                    Vector3 pointOnEdge = myCollider.bounds.center - directionToCenter * myCollider.bounds.extents.magnitude;
                    transform.position = Vector3.Lerp(transform.position, pointOnEdge, speed * Time.deltaTime);
                    isReturning = true;
                }
            }
        }
        
        if (vida <= 0)
        {
            if (!muerto)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(clipMuerte);
                
            }
            animator.SetBool("Muerto", true);
            muerto = true;
            collider.enabled = false;
            Invoke("Destruir",clipMuerte.length);
        }

    }
    private void Destruir()
    {
        Destroy(gameObject);
    }
    IEnumerator espera()
    {
        detener = true;
        yield return new WaitForSeconds(1);
        detener = false;
        isCharging = false;
        
    }

    public void ResivirDaño(int daño)
    {
        vida -= daño;
        resibiendoDano = true;
        isCharging = false;
        animator.SetBool("Atacando", false);
    }
}
