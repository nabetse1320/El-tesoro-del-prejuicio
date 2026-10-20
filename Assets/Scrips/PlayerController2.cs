using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2 : MonoBehaviour
{
    private Mapa _myInput;
    public Vector2 moveVector = Vector2.zero;
    private Rigidbody2D rb;
    public Animator animator;
    private CapsuleCollider2D[] colliders;
    public bool isGrounded;
    private float TiempoEntreAtaque;
    private float TiempoSiguienteAtaque;
    public bool isActive;
    public bool pausePlayer;
    [HideInInspector] public bool muerto;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isFalling;

    [SerializeField] private AnimationClip atrackClip;
    [SerializeField] private Transform controladorGolpe;
    [SerializeField] private float radioGolpe;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 1f;
    [Tooltip("Margen bajo los pies para detectar el suelo mientras esta en el aire")]
    [SerializeField] private float margenSueloEnElAire = 0.15f;
    private float distanciaSueloEnElAire;
    private PhysicsMaterial2D materialSuelo;
    private PhysicsMaterial2D materialAire;
    private readonly RaycastHit2D[] resultadosCast = new RaycastHit2D[8];
    [SerializeField] private float jumpForce;
    [SerializeField] private float speed;
    [SerializeField] private int dañoGolpe;

    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private AudioClip clipResivirDano;

    [SerializeField] private Interactor interactor;


    private bool isIdle = true;
    private bool isIdleE;
    public bool IsIdle
    {
        get { return isIdle; }
        set { isIdle = value; }
    }


    private void Awake()
    {
        colliders = GetComponents<CapsuleCollider2D>();
        // Se usa siempre el collider de suelo y solo se cambia su material (con o sin friccion).
        // Antes se alternaba con el collider de aire, que tiene los pies mas arriba, y al
        // volver al suelo la fisica empujaba al personaje hacia arriba (tirones en camara).
        materialSuelo = colliders[0].sharedMaterial;
        materialAire = colliders[1].sharedMaterial;
        colliders[0].enabled = true;
        colliders[1].enabled = false;
        float distanciaPies = (colliders[0].size.y / 2f - colliders[0].offset.y) * Mathf.Abs(transform.lossyScale.y);
        distanciaSueloEnElAire = Mathf.Min(raycastDistance, distanciaPies + margenSueloEnElAire);
        isActive = false;
        controladorGolpe = transform.GetChild(0).GetComponent<Transform>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        TiempoEntreAtaque = atrackClip.length;

        _myInput = new Mapa();
        _myInput.Enable();


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
        //_myInput.Player.Atacar.started += OnAtackStarted;
        //_myInput.Player.Jump.performed += OnJumpPerformed;

        //Eventos
        Eventos.eve.PausarPlayer2.AddListener(PausarPlayer);
        Eventos.eve.DespausarPlayer2.AddListener(DesausarPlayer);
        Eventos.eve.perderVida.AddListener(Herido);
        Eventos.eve.MuertePlayer.AddListener(Muerto);
        Eventos.eve.RevivirPlayer.AddListener(Vivo);
    }
    private void OnDisable()
    {
        //_myInput.Player.Movimiento.performed -= OnMovementPerformed;
        //_myInput.Player.Movimiento.canceled -= OnMovementCancelled;
        //_myInput.Player.Jump.performed -= OnJumpPerformed;
        //_myInput.Player.Atacar.started -= OnAtackStarted;
        Eventos.eve.PausarPlayer2.RemoveListener(PausarPlayer);
        Eventos.eve.DespausarPlayer2.RemoveListener(DesausarPlayer);
        Eventos.eve.MuertePlayer.RemoveListener(Muerto);
        Eventos.eve.RevivirPlayer.RemoveListener(Vivo);
        //Eventos.eve.perderVida.RemoveListener(Herido);
    }

    private void FixedUpdate()
    {
        float velocidadX = moveVector.x * speed;
        // En el aire no se empuja contra un obstaculo: si no, la base redondeada del collider
        // se apoyaba en la esquina de cajas o bordes y el personaje quedaba "flotando"
        if (!isGrounded && velocidadX != 0f && BloqueadoEnDireccion(Mathf.Sign(velocidadX)))
        {
            velocidadX = 0f;
        }
        rb.linearVelocity = new Vector2(velocidadX, rb.linearVelocity.y);
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
        //DebugRaycast();
        isGrounded = CheckGrounded();

        if (pausePlayer)
        {
            moveVector = Vector2.zero;
        }
        // Friccion solo cuando esta quieto en el suelo. Al caminar, saltar o en el aire se usa
        // el material sin friccion: si no, al empujar contra una pared o caja la friccion
        // frenaba el salto. La velocidad horizontal ya se fija en FixedUpdate, asi que no resbala.
        bool usarFriccion = isGrounded && moveVector.x == 0f && rb.linearVelocity.y <= 0.01f;
        PhysicsMaterial2D material = usarFriccion ? materialSuelo : materialAire;
        if (colliders[0].sharedMaterial != material)
        {
            colliders[0].sharedMaterial = material;
        }

        bool isAlmostIdle = isGrounded && rb.linearVelocity.magnitude < 0.1f;

        // Actualizar isIdle a true si el personaje está en reposo, de lo contrario, actualizar a false
        isIdle = isAlmostIdle;


        isJumping = !isGrounded && rb.linearVelocity.y > 0;
        isFalling = !isGrounded && rb.linearVelocity.y < 0;

        if (isFalling || isJumping) animator.ResetTrigger("Atacar");

        if (TiempoSiguienteAtaque > 0)
        {
            TiempoSiguienteAtaque -= Time.deltaTime;
        }

        animator.SetBool("Callendo", isFalling);
        animator.SetBool("Saltando", isJumping);
    
    }
    public void OnJumpPerformed(InputAction.CallbackContext value) {
        
        if (value.performed) 
        {
            if (!pausePlayer&&isActive)
            {
                if (isGrounded)
                {
                    colliders[0].sharedMaterial = materialAire;
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
                animator.SetBool("Corriendo", true);
                if (moveVector.x < 0f)
                {
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }
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
        }
       
    }

    public void OnAtackStarted(InputAction.CallbackContext value)
    {
        if (value.started&&isActive)
        {
            if (TiempoSiguienteAtaque <= 0 && isActive && !muerto && !pausePlayer)
            {
                animator.SetTrigger("Atacar");
                if (isGrounded) { Golpe(); TiempoSiguienteAtaque = TiempoEntreAtaque; }
            }
        }
        
        
    }

    private void Golpe() 
    {
        Collider2D[] objetos = Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe);
        foreach (Collider2D colicionador in objetos) {
            if (colicionador.CompareTag("Enemy"))
            {
                if (colicionador.GetComponent<Enemigo>() != null)
                {
                    colicionador.GetComponent<Enemigo>().ResivirDaño(dañoGolpe);
                }
                if (colicionador.GetComponent<Cangrejo>() != null)
                {
                    colicionador.GetComponent<Cangrejo>().ResivirDaño(dañoGolpe);
                }
            }
            if (colicionador.CompareTag("ObInteract"))
            {
                if (colicionador.GetComponent<Cuerda>()!=null)
                {
                    colicionador.GetComponent<Cuerda>().Desactivar();
                }
                if(colicionador.GetComponent <ProducirSonido>() != null)
                {
                    colicionador.GetComponent<ProducirSonido>().Sonar();
                }
                if(colicionador.GetComponent<SoundGame>() != null)
                {
                    StartCoroutine(colicionador.GetComponent<SoundGame>().PlaySequence());
                }
            }
            if (colicionador.CompareTag("Boss"))
            {

                colicionador.GetComponent<BossStats>().TakeDamage(1);
            }
        }
    }


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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(controladorGolpe.position, radioGolpe);
    }


    //void DebugRaycast()
    //{
    //    Vector2 raycastOrigin = transform.position;
    //    Debug.DrawRay(raycastOrigin, Vector2.down * raycastDistance, Color.red);
    //}


}
