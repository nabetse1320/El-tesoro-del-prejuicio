using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cangrejo : MonoBehaviour, IEnganchable
{
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 vectorPosicionRaycast;
    private bool atacando;
    private bool detected;
    private bool muerto;
    private bool enganchado;          // lo esta arrastrando el gancho de Izzy
    private float finAturdimiento;    // Time.time hasta el que sigue aturdido tras el gancho

    [SerializeField] private int vida;
    [SerializeField] private float stunTime;

    [Header("Configuraciones de detección al jugador:")]
    [SerializeField] private float detectionRadiusFirst;
    [SerializeField] private float detectionRadiusSecond;
    [SerializeField] private float timeToDetect;
    [SerializeField] private LayerMask playerMask;

    [Header("Configuraciones de movimiento:")]
    
    [SerializeField] private float movementVelocity;
    [SerializeField] private float distanciaPared;
    [SerializeField] private Transform inicioRaycastGround;
    [SerializeField] private float raycastDistancia2;
    [SerializeField] private LayerMask queEsSuelo;

    [Header("Configuraciones de ataque:")]
    [SerializeField] private AnimationClip detectClip;
    [Tooltip("Velocidad constante del embiste (unidades por segundo)")]
    [SerializeField] private float atackVelocity;
    [SerializeField] private float atackTime;
    private float tiempoFinAtaque;

    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip clipCaminata;
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private AudioClip clipDano;

    private bool alerted = false;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        muerto = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!muerto && Aturdido)
        {
            // Aturdido tras el gancho: no se mueve ni embiste (mientras lo arrastran, el gancho
            // controla su velocidad)
            if (!enganchado)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
        else if (!muerto) 
        {
            //DebugRaycast();
            isGrounded = CheckGrounded();
            RaycastHit2D informacionSuelo = Physics2D.Raycast(vectorPosicionRaycast, transform.right, distanciaPared, queEsSuelo);
            vectorPosicionRaycast = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            RaycastHit2D[] raycastHits = Physics2D.RaycastAll(vectorPosicionRaycast, transform.right, detectionRadiusFirst);
            foreach (var hit in raycastHits)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player1") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Player2"))
                {
                    if (!alerted)
                    {
                        animator.SetBool("Acostado", true);

                        StartCoroutine(Detect());
                    }
                    if (detected && !atacando && animator.GetBool("Acostado"))
                    {
                        atacando = true;
                        alerted = true;
                        tiempoFinAtaque = Time.time + atackTime;
                        detectionRadiusFirst = detectionRadiusSecond;
                    }
                    break;
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Terreno"))
                {
                    break;
                }
            }
            if (atacando)
            {
                if (Time.time >= tiempoFinAtaque)
                {
                    atacando = false;
                }
                else if (Time.timeScale != 0)
                {
                    // Embiste a velocidad constante, sin depender de la distancia al jugador
                    rb.linearVelocity = new Vector2(transform.right.x * atackVelocity, rb.linearVelocity.y);
                }
            }
            if (!atacando && alerted)
            {
                if (clipCaminata != null)
                {
                    if (!audioSource.isPlaying)
                    {
                        audioSource.clip = clipCaminata;
                        audioSource.Play();
                    }

                }
                rb.linearVelocity = new Vector2(movementVelocity * transform.right.x, rb.linearVelocity.y);
                animator.SetBool("caminando", true);

            }
            if (informacionSuelo || !isGrounded)
            {
                Girar();
            }
        }
        
        if (vida <= 0&&!muerto)
        {
            muerto = true;
            animator.SetBool("Muerte",true);
            Destroy(gameObject,1.5f);
        }
    }
    IEnumerator Detect()
    {
        yield return new WaitForSeconds(timeToDetect /*detectClip.length+1f*/);
        detected= true;
    }


    private void Girar()
    {
        if (transform.eulerAngles.y == 0)
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

    }

    public Rigidbody2D Cuerpo
    {
        get { return rb; }
    }

    public bool PuedeEngancharse
    {
        get { return !muerto && isActiveAndEnabled && rb != null && rb.bodyType == RigidbodyType2D.Dynamic; }
    }

    public bool Aturdido
    {
        get { return enganchado || Time.time < finAturdimiento; }
    }

    public void Enganchar()
    {
        if (muerto) return;
        enganchado = true;
        // Cancela el embiste y la deteccion que estuviera haciendo
        StopAllCoroutines();
        atacando = false;
        animator.SetBool("caminando", false);
        if (audioSource != null && audioSource.clip == clipCaminata) audioSource.Stop();
    }

    public void SoltarEnganche(float tiempoAturdido)
    {
        if (!enganchado) return;
        enganchado = false;
        finAturdimiento = Time.time + tiempoAturdido;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        // Al recuperarse sigue persiguiendo al jugador, como cuando recibe un golpe
        detected = true;
        alerted = true;
    }

    public void ResivirDaño(int daño)
    {
        vida -= daño;
        StopAllCoroutines();
        detected = false;
        alerted = false;
        animator.SetTrigger("Dañado");
        if (clipDano !=null)
        {
            audioSource.clip= clipDano;
            audioSource.Play();
        }
        StartCoroutine(WaitDamage());
    }
    IEnumerator WaitDamage()
    {
        yield return new WaitForSeconds(stunTime);
        detected = true;
        alerted= true;
        
    }

    private bool CheckGrounded()
    {
        Vector2 raycastOrigin = inicioRaycastGround.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, raycastDistancia2, queEsSuelo);
        return (hit.collider != null);
    }

    //void DebugRaycast()
    //{
    //    Vector2 raycastOrigin = inicioRaycastGround.transform.position;
    //    Debug.DrawRay(raycastOrigin, Vector2.down* raycastDistancia2, Color.blue);
    //}
}
