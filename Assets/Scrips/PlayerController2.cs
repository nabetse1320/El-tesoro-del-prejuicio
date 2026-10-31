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
    public bool isActive;
    public bool pausePlayer;
    [HideInInspector] public bool muerto;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isFalling;

    // Ya no se usa para medir el ataque: la duracion de cada paso esta en duracionAtaque[]
    [SerializeField] private AnimationClip atrackClip;

    [Header("Combo de espada (Lian)")]
    [Tooltip("Duracion total de cada paso del combo en segundos (paso 1, paso 2, paso 3 = estocada)")]
    [SerializeField] private float[] duracionAtaque = { 0.5f, 0.45f, 0.95f };
    [Tooltip("Segundos desde que empieza el paso en los que se hace el golpe (Golpe)")]
    [SerializeField] private float[] tiempoImpacto = { 0.25f, 0.2333f, 0.2333f };
    [Tooltip("Segundos desde que empieza el paso a partir de los cuales una pulsacion guardada encadena el siguiente paso. El paso 3 no encadena")]
    [SerializeField] private float[] tiempoEncadenar = { 0.31f, 0.2933f, 0f };
    [Tooltip("Segundos despues de terminar un paso sin pulsar en los que aun se puede pulsar para seguir el combo. Si se pasa, el combo vuelve al paso 1")]
    [SerializeField] private float ventanaCombo = 0.35f;
    [Tooltip("Segundos desde que empieza la estocada (paso 3) en los que acaba la pausa del ultimo frame del golpe. Si se pulso ataque durante la estocada, aqui se reinicia el combo sin volver a la pose de idle")]
    [SerializeField] private float finPausaEstocada = 0.5833f;
    [Tooltip("Enfriamiento en segundos tras terminar la estocada (paso 3) antes de poder empezar otro combo")]
    [SerializeField] private float enfriamientoFinal = 0.1f;

    private static readonly int[] hashesAtaque =
    {
        Animator.StringToHash("Ataque1"),
        Animator.StringToHash("Ataque2"),
        Animator.StringToHash("Ataque3")
    };
    private Coroutine rutinaCombo;
    private bool comboEnCurso;      // hay una rutina de combo activa
    private bool atacando;          // un paso del combo se esta reproduciendo
    private bool ventanaAbierta;    // el paso anterior termino y se puede seguir el combo
    private bool ataqueEnCola;      // se ha pulsado ataque durante el combo
    private int pasoActual;         // 1..3 mientras hay combo, 0 si no
    private float finEnfriamiento;  // Time.time hasta el que no se puede empezar un combo

    [SerializeField] private Transform controladorGolpe;
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
    [Tooltip("Margen (s) para poder saltar justo despues de salir de un borde")]
    [SerializeField] private float coyoteTime = 0.1f;
    [Tooltip("Si se pulsa saltar hasta estos segundos antes de aterrizar, salta al tocar el suelo")]
    [SerializeField] private float bufferSalto = 0.12f;
    [Tooltip("Al soltar saltar mientras sube, su velocidad vertical se multiplica por esto (1 = todos los saltos iguales)")]
    [Range(0.2f, 1f)]
    [SerializeField] private float multiplicadorSaltoCorto = 0.5f;
    private float ultimoSuelo = -10f;   // Time.time de la ultima vez que estaba en el suelo
    private float saltoPedido = -10f;   // Time.time de la ultima pulsacion de saltar
    private bool saltoEnCurso;          // subiendo por un salto del jugador (se puede cortar)
    private bool cortePendiente;        // se solto saltar antes de que empezara a subir
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
        isActive = false;
        controladorGolpe = transform.GetChild(0).GetComponent<Transform>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        duracionAtaque = AsegurarTres(duracionAtaque, 0.5f, 0.45f, 0.95f);
        tiempoImpacto = AsegurarTres(tiempoImpacto, 0.25f, 0.2333f, 0.2333f);
        tiempoEncadenar = AsegurarTres(tiempoEncadenar, 0.31f, 0.2933f, 0f);

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
        Eventos.eve.perderVida.RemoveListener(Herido);

        // Al desactivarse el objeto se paran las corrutinas: limpiar el estado del combo
        CancelarCombo();
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
        //DebugRaycast();
        isGrounded = CheckGrounded();

        if (pausePlayer)
        {
            moveVector = Vector2.zero;
        }

        // Si el combo esta activo y el personaje deja de poder atacar (muerte, pausa, cambio de
        // personaje o salto/caida) o empieza a caminar, se interrumpe el combo
        if (comboEnCurso && !PuedeAtacar())
        {
            CancelarCombo();
        }

        bool isAlmostIdle = isGrounded && rb.linearVelocity.magnitude < 0.1f;

        // Actualizar isIdle a true si el personaje está en reposo, de lo contrario, actualizar a false
        isIdle = isAlmostIdle;


        // Al salir de un borde caminando no cuenta como caida hasta llevar un instante en el aire
        // (evita parpadeos de la pose al bajar escalones o pendientes)
        tiempoSinSuelo = isGrounded ? 0f : tiempoSinSuelo + Time.deltaTime;

        // Coyote time y buffer de salto. Justo despues de saltar el rayo aun ve el suelo un
        // instante: se espera un poco para no permitir un segundo salto
        if (isGrounded && tiempoDesdeSalto > 0.15f)
        {
            ultimoSuelo = Time.time;
            saltoEnCurso = false;
        }
        if (saltoEnCurso && rb.linearVelocity.y <= 0f && tiempoDesdeSalto > 0.1f)
        {
            saltoEnCurso = false;
            cortePendiente = false;
        }
        if (cortePendiente && rb.linearVelocity.y > 0f) CortarSalto();
        IntentarSaltar();

        isJumping = !isGrounded && rb.linearVelocity.y > 0;
        isFalling = !isGrounded && rb.linearVelocity.y < 0 && tiempoSinSuelo >= margenCaida;
        if (tiempoPoseAire > 0f)
        {
            // Recien soltado de una cuerda: aun sin velocidad vertical ya esta cayendo
            tiempoPoseAire -= Time.deltaTime;
            if (!isGrounded && !isJumping) isFalling = true;
        }

        animator.SetBool("Callendo", isFalling);
        animator.SetBool("Saltando", isJumping);
    
    }
    public void OnJumpPerformed(InputAction.CallbackContext value) {
        
        if (value.performed)
        {
            if (!pausePlayer&&isActive)
            {
                // Se recuerda la pulsacion: si aun no toca el suelo, salta al aterrizar (buffer)
                saltoPedido = Time.time;
                IntentarSaltar();
                isIdle = false;
            }
        }
        else if (value.canceled)
        {
            CortarSalto();
        }

    }

    // Salta si esta en el suelo o acaba de dejarlo (coyote time) y hay un salto pedido hace poco
    private void IntentarSaltar()
    {
        if (pausePlayer || !isActive) return;
        if (Time.time - saltoPedido > bufferSalto) return;
        if (Time.time - ultimoSuelo > coyoteTime) return;

        saltoPedido = -10f;
        ultimoSuelo = -10f; // no se puede volver a saltar hasta tocar el suelo otra vez
        // El salto siempre parte de velocidad vertical 0 (en pendientes se sumaba)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        tiempoDesdeSalto = 0f;
        rb.AddForce(Vector2.up * jumpForce);
        saltoEnCurso = true;
    }

    // Al soltar el boton mientras sube, el salto se queda mas corto
    private void CortarSalto()
    {
        if (!saltoEnCurso) return;
        if (rb.linearVelocity.y > 0f)
        {
            saltoEnCurso = false;
            cortePendiente = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * multiplicadorSaltoCorto);
        }
        else
        {
            // Soltado antes de que la fisica aplicara el impulso: se corta en cuanto empiece a subir
            cortePendiente = true;
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

    // True mientras se reproduce un paso del combo (no cuenta la ventana de espera entre pasos)
    public bool EstaAtacando
    {
        get { return atacando; }
    }

    public void OnAtackStarted(InputAction.CallbackContext value)
    {
        if (!value.started) return;
        if (!PuedeAtacar()) return;

        if (comboEnCurso)
        {
            // Pulsacion durante el combo: se guarda para encadenar el siguiente paso.
            // Durante la ventana entre pasos tambien se guarda (la rutina la usa para seguir el combo).
            // Durante la estocada (paso 3) se guarda para reiniciar el combo al acabar la pausa
            if (atacando || ventanaAbierta)
            {
                ataqueEnCola = true;
            }
            return;
        }

        if (Time.time < finEnfriamiento) return;

        comboEnCurso = true;
        rutinaCombo = StartCoroutine(RutinaCombo(1));
    }

    private IEnumerator RutinaCombo(int paso)
    {
        while (true)
        {
            int i = paso - 1;
            pasoActual = paso;
            atacando = true;
            ventanaAbierta = false;
            ataqueEnCola = false;
            animator.Play(hashesAtaque[i], 0, 0f);

            float duracion = Mathf.Max(0.01f, duracionAtaque[i]);
            // El golpe no puede ser posterior al final del paso
            float impacto = Mathf.Min(tiempoImpacto[i], duracion);
            bool golpeado = false;
            bool encadenar = false;
            bool reiniciar = false;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                yield return null;
                if (!PuedeAtacar())
                {
                    ResetearCombo();
                    yield break;
                }
                tiempo += Time.deltaTime;

                if (!golpeado && tiempo >= impacto)
                {
                    golpeado = true;
                    Golpe();
                }
                if (paso < 3 && ataqueEnCola && tiempo >= tiempoEncadenar[i])
                {
                    encadenar = true;
                    break;
                }
                // Tras la pausa de la estocada, si se pulso ataque, se salta la vuelta a la pose
                // de idle y el combo empieza otra vez desde el paso 1
                if (paso >= 3 && ataqueEnCola && tiempo >= finPausaEstocada)
                {
                    reiniciar = true;
                    break;
                }
            }
            // Cada paso hace su golpe exactamente una vez
            if (!golpeado) Golpe();

            if (encadenar)
            {
                paso++;
                continue;
            }
            if (reiniciar)
            {
                paso = 1;
                continue;
            }

            // El paso termino sin pulsacion guardada
            atacando = false;

            if (paso >= 3)
            {
                // Fin de la estocada: enfriamiento y vuelta al paso 1
                finEnfriamiento = Time.time + enfriamientoFinal;
                ResetearCombo();
                yield break;
            }

            // Ventana para pulsar y continuar con el siguiente paso
            ventanaAbierta = true;
            float espera = 0f;
            while (espera < ventanaCombo && !ataqueEnCola)
            {
                yield return null;
                if (!PuedeAtacar())
                {
                    ResetearCombo();
                    yield break;
                }
                espera += Time.deltaTime;
            }
            ventanaAbierta = false;

            if (!ataqueEnCola)
            {
                // Se acabo la ventana: el combo vuelve al paso 1
                ResetearCombo();
                yield break;
            }

            paso++;
        }
    }

    private bool PuedeAtacar()
    {
        // No se ataca caminando: la animacion de ataque deja las piernas estaticas
        return isActive && !muerto && !pausePlayer && isGrounded && Mathf.Abs(moveVector.x) <= 0.01f;
    }

    private void CancelarCombo()
    {
        if (rutinaCombo != null) StopCoroutine(rutinaCombo);
        ResetearCombo();
    }

    private void ResetearCombo()
    {
        rutinaCombo = null;
        comboEnCurso = false;
        atacando = false;
        ventanaAbierta = false;
        ataqueEnCola = false;
        pasoActual = 0;
    }

    private static float[] AsegurarTres(float[] valores, float a, float b, float c)
    {
        // Protege contra arrays vacios o incompletos (p. ej. si se editan en el Inspector)
        if (valores == null || valores.Length < 3) return new float[] { a, b, c };
        return valores;
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

    // Al soltarse de una cuerda: pone ya en este frame la pose de salto o caida si esta en el aire
    // (con velocidad vertical 0 el Animator pasaria a Idle y desde ahi no llega a la caida)
    private float tiempoPoseAire;
    private float tiempoSinSuelo;
    private const float margenCaida = 0.08f;

    public void ActualizarPoseAire(Vector2 velocidad)
    {
        tiempoPoseAire = 0.2f;
        isGrounded = CheckGrounded();
        animator.SetBool("Saltando", !isGrounded && velocidad.y > 0f);
        animator.SetBool("Callendo", !isGrounded && velocidad.y <= 0f);
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
