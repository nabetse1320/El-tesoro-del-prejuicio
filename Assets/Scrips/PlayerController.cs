using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    private Mapa _myInput;
    public Vector2 moveVector = Vector2.zero;
    private Rigidbody2D rb;
    public Animator animator;
    //private CapsuleCollider2D collider;
    private CapsuleCollider2D[] colliders; 
    public bool isGrounded;
    //private float TiempoEntreAtaque;
    private float TiempoSiguienteAtaque;
    public bool isActive;
    public bool pausePlayer;
    [HideInInspector] public bool muerto;
    [SerializeField] private int vida=10;
    [SerializeField] private AnimationClip atrackClip;
    //[SerializeField] private Transform controladorGolpe;
    [SerializeField] private float radioGolpe;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 1f;
    [Tooltip("Margen bajo los pies para detectar el suelo mientras esta en el aire")]
    [SerializeField] private float margenSueloEnElAire = 0.15f;
    private float distanciaSueloEnElAire;
    private PhysicsMaterial2D materialSinFriccion;
    [Tooltip("Tiempo que tarda el collider en pasar de la forma de suelo a la de salto (piernas recogidas) y viceversa")]
    [SerializeField] private float tiempoCambioCollider = 0.08f;
    private Vector2 offsetSuelo, sizeSuelo, offsetAire, sizeAire;
    private float formaAire; // 0 = forma de suelo, 1 = forma de salto
    private float distanciaPies;
    [Tooltip("Inclinacion maxima (en grados) que se considera suelo para caminar por pendientes")]
    [SerializeField] private float pendienteMaxima = 55f;
    private readonly ContactPoint2D[] contactos = new ContactPoint2D[16];
    private Joint2D[] joints;
    private float tiempoDesdeSalto = 1f;
    private readonly RaycastHit2D[] resultadosCast = new RaycastHit2D[8];
    [SerializeField] private float jumpForce;
    [SerializeField] private float speed;
    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private AudioClip clipResivirDano;
    //[SerializeField] private int dañoGolpe;
    //[SerializeField] private PlayerInput;

    [SerializeField] private Interactor interactor;
    
    private bool isIdle = true;
    public bool IsIdle
    {
        get { return isIdle; }
        set { isIdle = value; }
    }



    private void Awake()
    {
        colliders = GetComponents<CapsuleCollider2D>();
        // Se usa un solo collider que cambia de forma de manera gradual entre la del collider de
        // suelo y la del collider de aire (pies mas arriba, como las piernas recogidas del salto).
        // Antes se activaba uno u otro de golpe y al aterrizar el de suelo aparecia metido en el
        // piso, la fisica empujaba al personaje hacia arriba y la camara daba tirones.
        // El personaje nunca usa friccion: con friccion se quedaba pegado o resbalaba lento
        // contra paredes y cajas, y la velocidad horizontal ya se fija en FixedUpdate.
        materialSinFriccion = colliders[1].sharedMaterial;
        colliders[0].sharedMaterial = materialSinFriccion;
        GetComponent<Rigidbody2D>().sharedMaterial = materialSinFriccion;
        offsetSuelo = colliders[0].offset;
        sizeSuelo = colliders[0].size;
        offsetAire = colliders[1].offset;
        sizeAire = colliders[1].size;
        colliders[0].enabled = true;
        colliders[1].enabled = false;
        distanciaPies = (sizeSuelo.y / 2f - offsetSuelo.y) * Mathf.Abs(transform.lossyScale.y);
        joints = GetComponents<Joint2D>();
        distanciaSueloEnElAire = Mathf.Min(raycastDistance, distanciaPies + margenSueloEnElAire);
        //colliders[0].sharedMaterial.friction = 1;
        //colliders[1].sharedMaterial.friction = 0;
        audioSource = GetComponent<AudioSource>();
        isActive = true;
        //controladorGolpe = transform.GetChild(0).GetComponent<Transform>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        //collider =GetComponent<CapsuleCollider2D>();


        //TiempoEntreAtaque = atrackClip.length;

        // No se crea una copia propia de las acciones de entrada: no se usaba (el control llega por
        // PlayerInput) y cada copia habilitada procesaba todos los eventos de entrada en cada frame.
        //_myInput = new Mapa();
        //_myInput.Enable();
    }

    private void OnDestroy()
    {
        // Liberar las acciones de entrada creadas en Awake (evita fugas al cambiar de escena)
        _myInput?.Disable();
        _myInput?.Dispose();
    }

    private void OnEnable()
    {
        ////Movimiento
        //_myInput.Player.Movimiento.performed += OnMovementPerformed;
        //_myInput.Player.Movimiento.canceled += OnMovementCancelled;

        ////Otras acciones
        ////_myInput.Player.Atacar.performed += OnAtackPerformed;
        //_myInput.Player.Jump.performed += OnJumpPerformed;

        //Eventos
        Eventos.eve.DespausarPlayer.AddListener(DesausarPlayer);
        Eventos.eve.PausarPlayer.AddListener(PausarPlayer);
        Eventos.eve.perderVida.AddListener(Herido);
        Eventos.eve.MuertePlayer.AddListener(Muerto);
        Eventos.eve.RevivirPlayer.AddListener(Vivo);
    }

    private void OnDisable()
    {
        //_myInput.Player.Movimiento.performed -= OnMovementPerformed;
        //_myInput.Player.Movimiento.canceled -= OnMovementCancelled;
        //_myInput.Player.Jump.performed -= OnJumpPerformed;

        Eventos.eve.DespausarPlayer.RemoveListener(DesausarPlayer);
        Eventos.eve.PausarPlayer.RemoveListener(PausarPlayer);
        Eventos.eve.perderVida.RemoveListener(Herido);
        Eventos.eve.MuertePlayer.RemoveListener(Muerto);
        Eventos.eve.RevivirPlayer.RemoveListener(Vivo);
    }

    private void FixedUpdate()
    {
        ActualizarFormaCollider();
        float velocidadX = moveVector.x * speed;
        tiempoDesdeSalto += Time.fixedDeltaTime;

        // En el suelo se camina a lo largo de la superficie a velocidad constante y se compensa
        // la gravedad: sin friccion, en pendientes el personaje se aceleraba al bajar y al subir
        // ganaba velocidad vertical que se sumaba al salto (saltos muy altos)
        Vector2 normalSuelo, velocidadSuelo;
        if (tiempoDesdeSalto > 0.2f && PuedeAjustarseAlSuelo() && ObtenerNormalSuelo(out normalSuelo, out velocidadSuelo))
        {
            Vector2 tangente = new Vector2(normalSuelo.y, -normalSuelo.x);
            // Se suma la velocidad de lo que pisa (plataformas o cajas en movimiento)
            rb.linearVelocity = tangente * velocidadX + velocidadSuelo;
            rb.AddForce(-Physics2D.gravity * rb.gravityScale * rb.mass);
            return;
        }

        // En el aire no se empuja contra un obstaculo: si no, la base redondeada del collider
        // se apoyaba en la esquina de cajas o bordes y el personaje quedaba "flotando"
        if (!isGrounded && velocidadX != 0f && BloqueadoEnDireccion(Mathf.Sign(velocidadX)))
        {
            velocidadX = 0f;
        }
        rb.linearVelocity = new Vector2(velocidadX, rb.linearVelocity.y);
    }

    private void ActualizarFormaCollider()
    {
        // Forma de suelo si esta en el suelo o si va cayendo y el suelo esta cerca (asi recupera
        // la forma antes de tocarlo y no aparece metido en el piso). Subiendo o lejos del suelo
        // usa la forma de salto, para no engancharse con los bordes de las plataformas.
        bool sueloCerca = rb.linearVelocity.y <= 0f
            && Physics2D.Raycast(transform.position, Vector2.down, distanciaPies + 0.3f, groundLayer).collider != null;
        float objetivo = (isGrounded || sueloCerca) ? 0f : 1f;
        float nueva = Mathf.MoveTowards(formaAire, objetivo, Time.fixedDeltaTime / Mathf.Max(0.001f, tiempoCambioCollider));
        if (nueva != formaAire)
        {
            formaAire = nueva;
            colliders[0].offset = Vector2.Lerp(offsetSuelo, offsetAire, formaAire);
            colliders[0].size = Vector2.Lerp(sizeSuelo, sizeAire, formaAire);
        }
    }

    private bool PuedeAjustarseAlSuelo()
    {
        // Gancho, lianas y cuerdas controlan al personaje quitandole la gravedad o con joints
        if (rb.gravityScale == 0f) return false;
        foreach (Joint2D joint in joints)
        {
            if (joint.enabled) return false;
        }
        return true;
    }

    private bool ObtenerNormalSuelo(out Vector2 normal, out Vector2 velocidadSuelo)
    {
        normal = Vector2.up;
        velocidadSuelo = Vector2.zero;
        float minimoY = Mathf.Cos(pendienteMaxima * Mathf.Deg2Rad);
        bool encontrado = false;
        int cantidad = colliders[0].GetContacts(contactos);
        for (int i = 0; i < cantidad; i++)
        {
            // La normal apunta hacia otherCollider; se orienta siempre hacia el personaje
            bool invertido = contactos[i].collider == colliders[0];
            Collider2D otro = invertido ? contactos[i].otherCollider : contactos[i].collider;
            if (otro == null || otro.isTrigger) continue;
            Vector2 n = invertido ? -contactos[i].normal : contactos[i].normal;
            if (n.y >= minimoY && (!encontrado || n.y > normal.y))
            {
                normal = n;
                velocidadSuelo = otro.attachedRigidbody != null ? otro.attachedRigidbody.GetPointVelocity(contactos[i].point) : Vector2.zero;
                encontrado = true;
            }
        }
        return encontrado;
    }

    private bool BloqueadoEnDireccion(float direccion)
    {
        ContactFilter2D filtro = new ContactFilter2D();
        filtro.useTriggers = false;
        filtro.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        int cantidad = colliders[0].Cast(new Vector2(direccion, 0f), filtro, resultadosCast, 0.05f);
        for (int i = 0; i < cantidad; i++)
        {
            // Superficie que se opone al avance (pared o esquina)
            if (resultadosCast[i].normal.x * direccion < -0.1f)
            {
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        DebugRaycast();
        isGrounded = CheckGrounded();
        

        bool isAlmostIdle = isGrounded && rb.linearVelocity.magnitude < 0.1f;

        // Actualizar isIdle a true si el personaje está en reposo, de lo contrario, actualizar a false
        isIdle = isAlmostIdle;

        if (!IsIdle)
        {
            if ((moveVector.x < 0f && transform.eulerAngles.y != 0) || (moveVector.x > 0f && transform.eulerAngles.y == 0))
            {
                animator.SetBool("Corriendo", true);
                animator.SetBool("CorriendoEspalda", false);
            }
            else if ((moveVector.x < 0f && transform.eulerAngles.y == 0) || (moveVector.x > 0f && transform.eulerAngles.y != 0))
            {
                animator.SetBool("Corriendo", false);
                animator.SetBool("CorriendoEspalda", true);
            }
        }

        if (pausePlayer) 
        {
            moveVector = Vector2.zero;
        }


        //collider.sharedMaterial.friction = isGrounded ? 1 : 0; ;

        bool isJumping = !isGrounded && rb.linearVelocity.y > 0;
        bool isFalling = !isGrounded && rb.linearVelocity.y < 0;

        if (isFalling || isJumping) animator.ResetTrigger("Atacar");
        
        if (TiempoSiguienteAtaque > 0) { 
            TiempoSiguienteAtaque -= Time.deltaTime;
        }

        animator.SetBool("Callendo", isFalling);
        animator.SetBool("Saltando", isJumping);
    
    }
    public void OnJumpStarted(InputAction.CallbackContext value)
    {

        if (value.started)
        {
            if (!pausePlayer && isActive)
            {
                if (isGrounded)
                {
                    // El salto siempre parte de velocidad vertical 0 (en pendientes se sumaba)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    tiempoDesdeSalto = 0f;
                    rb.AddForce(Vector2.up * jumpForce);
                }

                isIdle = false;
            }
        }

    }

    public void OnMovementPerformed(InputAction.CallbackContext value)
    {

        if (value.performed)
        {
            if (!pausePlayer)
            {
                moveVector = value.ReadValue<Vector2>();
                isIdle = false;

            }
            else
            {
                // Si el player está en pausa (modo ocio), no está realizando ninguna acción activa.
                // Entonces, actualiza isIdle a true.
                isIdle = true;
            }
        }
        


    }

    public void OnMovementCancelled(InputAction.CallbackContext value)
    {
        if (value.canceled)
        {
            moveVector = Vector2.zero;
            animator.SetBool("Corriendo", false);
            animator.SetBool("CorriendoEspalda", false);
        }
        

    }

    //private void OnAtackPerformed(InputAction.CallbackContext value)
    //{
    //    if (value.performed && TiempoSiguienteAtaque<=0)
    //    {
    //        animator.SetTrigger("Atacar");
    //        Golpe();
    //       TiempoSiguienteAtaque = TiempoEntreAtaque;
    //    } 
        
    //}



    //private void Golpe() 
    //{
    //    Collider2D[] objetos = Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe);
    //    foreach (Collider2D colicionador in objetos) {
    //        if (colicionador.CompareTag("Enemy"))
    //        {
    //            colicionador.transform.GetComponent<Enemigo>().ResivirDaño(dañoGolpe);
    //        }
    //    }
    //}


    private bool CheckGrounded() {
        Vector2 raycastOrigin = transform.position;
        // En el aire el rayo es mas corto: solo detecta el suelo justo antes de tocarlo
        float distancia = isGrounded ? raycastDistance : distanciaSueloEnElAire;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, distancia, groundLayer);
        return (hit.collider != null);
    }
    private void Herido()
    {
        if (isActive)
        {
            audioSource.clip = clipResivirDano;
            audioSource.Play();
        }
    }

    public void PausarPlayer() 
    { 
        pausePlayer = true;
    }
    public void DesausarPlayer()
    {
        pausePlayer = false;
    }
    public void Muerto()
    {
        muerto = true;
        PausarPlayer();
    }
    public void Vivo()
    {
        muerto = false;
        DesausarPlayer();
    }

    public void OnInteractPerformed(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            if (isActive) 
            {
                interactor.Interact();
            }
            
        }
    }
    public void OnInteractCanceled(InputAction.CallbackContext value)
    {
        if (value.canceled)
        {
            if (isActive) 
            {
                interactor.EndInteract();
            }
        }
    }

    //private IEnumerator EsperarAnimacionMuerte()
    //{
    //    AnimationClip animacionMuerteClip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
    //    float duracionAnimacion = animacionMuerteClip.length;

    //    yield return new WaitForSeconds(duracionAnimacion);

    //    Destroy(gameObject);
    //}


    //private void OnDrawGizmos()
    //{
    //   Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(controladorGolpe.position, radioGolpe);
    //}


    void DebugRaycast()
    {
        Vector2 raycastOrigin = transform.position;
        Debug.DrawRay(raycastOrigin, Vector2.down * raycastDistance, Color.blue);
    }


}
