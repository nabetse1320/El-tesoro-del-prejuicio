using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.InputSystem.Controls.AxisControl;
using static UnityEngine.UI.Image;

public class Enemigo : MonoBehaviour
{
    
    private Rigidbody2D rb;
    private Animator animator;
    private bool muerto;
    [SerializeField] private bool isGrounded;
    private bool aRango;
    [SerializeField] private bool atacando;
    [SerializeField] public bool detectandoPlayer;
    [SerializeField] public bool playerDetectado;
    [SerializeField] private bool modoAlerta;
    public bool parado;
    private bool escondiendose;
    private bool golpeEjecutado = false;
    [HideInInspector] public Collider2D[] hits;
    [SerializeField] private bool modoAtaque;
    [SerializeField] private int vida;
    [SerializeField] public int daño;

    [Header("Configuraciones de movimiento:")]
    [SerializeField] private LayerMask otrosEnemigos;
    [SerializeField] private float velocidadMovimiento;
    [SerializeField] private float distanciaPared;
    [SerializeField] private float raycastDistancia2;
    [SerializeField] private Transform inicioRaycastGround;
    [SerializeField] private Transform inicioRaycastPared;
    [SerializeField] private LayerMask queEsSuelo;

    [Header("Configuraciones de detección al jugador:")]
    [SerializeField] private float detectionRadius;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float tEsperaAtaque;
    [SerializeField] private float tEsconderse;
    private Transform p;

    [Header("configuraciones de enemigos cuerpo a cuerpo:")]
    [SerializeField] private Transform controladorGolpe;
    [SerializeField] private float radioGolpe;

    [Header("Tipo de enemigo:")]
    [SerializeField] public bool isMobile;
    [SerializeField] private bool isCac;//cuerpo a cuerpo
    [SerializeField] private GameObject arma;

    [Header("Configuraciones de ataque:")]
    [SerializeField] private int dañoGolpe;
    [SerializeField] private float tEntreAtaques;
    [SerializeField] private float prueba;
    [SerializeField] private AnimationClip AnimacionAtaque;

    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private AudioClip clipResivirDano;




    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        p = GameObject.FindGameObjectWithTag("Player").transform;
        aRango = false;
        muerto = false;
        atacando = false;
        parado = false;
        detectandoPlayer = false;
        modoAlerta = false;
        playerDetectado = false;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        modoAtaque = false;
    }


    private void Update()
    {
        
        if (!muerto) 
        {
            
            DebugRaycast();
            isGrounded = CheckGrounded();
            hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerMask);
            RaycastHit2D informacionSuelo = Physics2D.Raycast(inicioRaycastPared.position, transform.right, distanciaPared, queEsSuelo);
            RaycastHit2D informacionPlayerCac = Physics2D.Raycast(inicioRaycastPared.position, transform.right, distanciaPared, playerMask);
            //ENEMIGOS PATRULLEROS A DISTANCIA
            if (!isCac)
            {
                arma.SetActive(true);
                foreach (var hit in hits)
                {
                    
                    Vector3 directionToPlayer = (hit.transform.position - transform.position).normalized;
                    DebugRaycast();
                    RaycastHit2D[] raycastHits = Physics2D.RaycastAll(transform.position, directionToPlayer, detectionRadius);
                    foreach (var raycastHit in raycastHits)
                    {

                        if (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Player1") || raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Player2"))
                        {
                            if (((directionToPlayer.x > 0 && transform.rotation.y == 0) || (directionToPlayer.x < 0 && transform.rotation.y != 0)) && !playerDetectado)
                            {
                                aRango = true;
                                //if (!detectandoPlayer && modoAtaque) { modoAtaque = false; }
                                parado = true;

                                if (!playerDetectado && !detectandoPlayer && !modoAtaque)
                                {
                                    animator.SetBool("Alerta",true);
                                    detectar();
                                }
                            }
                            if (playerDetectado || modoAlerta)
                            {
                                float angulo = 0;
                                if (directionToPlayer.x > 0)
                                {
                                    angulo = 0;
                                    // Rota hacia la derecha
                                }
                                else if (directionToPlayer.x < 0)
                                {
                                    angulo = 180; // Rota hacia la izquierda
                                }           
                                transform.rotation = Quaternion.AngleAxis(angulo, Vector3.up);
                                ////////////////////////////////////////////////////////////////
                                if (hit.CompareTag("Player") && !atacando)
                                {

                                    atacar();
                                }
                            }
                            break;
                        }
                        else if ((!isMobile && raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Terreno")&&!escondiendose) || isMobile && playerDetectado && modoAtaque && raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Terreno")&&!escondiendose)
                        {
                            
                            StartCoroutine(esconderse());
                            break;
                        }
                        else if (((!parado && !detectandoPlayer && modoAlerta) && isMobile) && raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Terreno"))
                        {
                            animator.SetBool("Alerta",false);
                            animator.SetBool("Atacando", false);
                            rb.velocity = new Vector2(velocidadMovimiento * 0.2f * transform.right.x, rb.velocity.y);
                            aRango = false;
                            break;
                        }
                        else if (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Terreno"))
                        {
                            break;
                        }
                    }
                }
                if (hits.Length == 0)
                {
                    parado = false;
                    modoAlerta = false;
                    modoAtaque = false;
                    playerDetectado = false;
                    aRango = false;
                }
            }
            else
            {
                if (playerDetectado)
                {
                    Vector3 directionToPlayer = (p.transform.position - transform.position).normalized;
                    float angulo = 0;
                    if (directionToPlayer.x > 0)
                    {
                        angulo = 0; // Rota hacia la derecha
                    }
                    else if (directionToPlayer.x < 0)
                    {
                        angulo = 180; // Rota hacia la izquierda
                    }
                    transform.rotation = Quaternion.AngleAxis(angulo, Vector3.up);
                }
                arma.SetActive(false);
            }
            /////////////////////////////////////////
            //ENEMIGOS PATRULLEROS CUERPO A CUERPO
            if (informacionPlayerCac && isCac && !atacando)
            {
                parado = true;
                atacar();
            }
            else if (isCac)
            {
                playerDetectado = false;
            }
            if (!parado && isMobile && !detectandoPlayer && !modoAlerta)
            {
                if (!isCac) 
                {
                    animator.SetBool("Alerta", false);
                    animator.SetBool("Atacando", false);
                }
                rb.velocity = new Vector2(velocidadMovimiento * transform.right.x, rb.velocity.y);
            }
            if (informacionSuelo || !isGrounded)
            {
                if (!aRango && !isCac)
                {
                    Girar();
                }
                else if (isCac)
                {
                    Girar();
                }

            }
            
        }
        if (!isCac && !isMobile)
        {
            animator.SetBool("Alerta", true);
        }
        if (isCac && !isMobile)
        {
            animator.SetBool("Atacando", false);
            animator.SetBool("Muerto", false);
            animator.SetBool("idle", true);
        }

        if (vida <= 0&&!muerto)
        {
            StopAllCoroutines();
            muerto= true;
            animator.SetBool("Muerto",true);
            animator.SetBool("Alerta", false);
            animator.SetBool("Atacando", false);
            animator.SetBool("daño", false);
            Destroy(gameObject,1.5f);
        }
    }
    IEnumerator esconderse()
    {
        escondiendose = true;
        yield return new WaitForSeconds(tEsconderse);
        playerDetectado = false;
        modoAtaque =false;
        parado = false;
        modoAlerta = true;
        escondiendose=false;
    }

    private bool CheckGrounded()
    {
        Vector2 raycastOrigin = inicioRaycastGround.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, raycastDistancia2, queEsSuelo);
        return (hit.collider != null);
    }

    private bool detectar()
    {
        detectandoPlayer = true;
        StartCoroutine(Detectado());
        return playerDetectado;
    }
    private IEnumerator Detectado()
    {
        yield return new WaitForSeconds(tEsperaAtaque);
        foreach (var hit in hits)
        {
            Vector3 directionToPlayer = (hit.transform.position - transform.position).normalized;
            RaycastHit2D[] raycastHits = Physics2D.RaycastAll(transform.position, directionToPlayer, detectionRadius);
            foreach (var raycastHit in raycastHits)
            {
                if (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Player1") || raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Player2"))
                {
                    if (hit.CompareTag("Player") /*&& !atacando*/)
                    {
                        playerDetectado = true;
                        modoAtaque = true;
                        modoAlerta = false;
                    }
                    break;
                }
                else if(raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Terreno"))
                {
                    
                    parado = false;
                    modoAlerta = true;
                    playerDetectado = false;
                    break;
                }
            }
        }
        detectandoPlayer = false;
    }


    private void atacar()
    {
        atacando = true;
        StartCoroutine(ataque());
    }

    private IEnumerator ataque()
    {

        animator.SetBool("Atacando", true);
        yield return new WaitForSeconds(AnimacionAtaque.length*0.5f);

        if (!golpeEjecutado) // Verificar nuevamente antes de ejecutar el golpe
        {
            golpeEjecutado = true;
            if (isCac) {
                if (clipAtaque != null)
                {
                    audioSource.clip = clipAtaque;
                    audioSource.Play();
                }
                Golpe();}
            else 
            {
                if (clipAtaque!=null)
                {
                    audioSource.clip = clipAtaque;
                    audioSource.Play();
                }
                Eventos.eve.disparoEnemigo.Invoke();
            }
            // Marcar el golpe como ejecutado
        }
        yield return new WaitForSeconds(tEntreAtaques);
        if (isCac)
        {
            parado = false;
        }
        golpeEjecutado = false;
        animator.SetBool("Atacando", false);
        atacando = false;
    }

    private void Golpe()
    {
        Collider2D[] objetos = Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe);
        foreach (Collider2D colicionador in objetos)
        {
            if (colicionador.CompareTag("Player"))
            {
                Eventos.eve.perderVida.Invoke();
            }
        }
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

    public void ResivirDaño(int daño)
    {
        if (!muerto)
        {
            vida -= daño;
            StopAllCoroutines();
            animator.SetBool("daño", true);
            StartCoroutine(WaitDamage());
        }
        
    }
    IEnumerator WaitDamage()
    {
        yield return new WaitForSeconds(tEntreAtaques);
        detectandoPlayer = false;
        playerDetectado = true;
        modoAtaque = true;
        golpeEjecutado = false;
        atacando = false;
        parado = true;
        animator.SetBool("daño", false);
        if (clipResivirDano != null)
        {
            audioSource.clip=clipResivirDano;
            audioSource.Play();
        }
        if (isCac)
        {
            parado = false;
        }
    }
    void DebugRaycast()
    {
        Vector2 raycastOrigin = inicioRaycastPared.position;
        Debug.DrawRay(raycastOrigin, Vector2.right*distanciaPared, Color.blue);
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, detectionRadius);
    //}

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(controladorGolpe.position, radioGolpe);
        //foreach (var hit in hits)
        //{
        //    Gizmos.color = Color.red;
        //    if (hit.CompareTag("Player"))
        //    {
        //        Gizmos.DrawLine(transform.position, (hit.transform.position - transform.position) * detectionRadius);
        //    }
        //}
    }
}
