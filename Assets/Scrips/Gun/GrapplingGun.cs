using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingGun : MonoBehaviour
{
    private Mapa inputs;
    [SerializeField] private PlayerController player;
    [Header("Scripts Ref:")]
    public GrapplingRope grappleRope;

    [Header("Layers Settings:")]
    [SerializeField] private bool grappleToAll = false;
    [SerializeField] private int grappableLayerNumber = 6;

    [Header("Main Camera:")]
    public Camera m_camera;

    [Header("Transform Ref:")]
    public Transform gunHolder;
    public Transform gunPivot;
    public Transform firePoint;

    [Header("Physics Ref:")]
    public SpringJoint2D m_springJoint2D;
    public Rigidbody2D m_rigidbody;

    [Header("Rotation:")]
    [SerializeField] private bool rotateOverTime = true;
    [Range(0, 60)][SerializeField] private float rotationSpeed = 4;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistnace = 20;
    [SerializeField] private float timeToGrapplin;
    [SerializeField] Transform lookPointObject;


    private enum LaunchType
    {
        Transform_Launch,
        Physics_Launch
    }

    [Header("Launching:")]
    [SerializeField] private bool launchToPoint = false;
    [SerializeField] private LaunchType launchType = LaunchType.Physics_Launch;
    [SerializeField] private float launchSpeed = 1;
    [SerializeField] public float coolDownTime;

    [Header("No Launch To Point")]
    [SerializeField] private bool autoConfigureDistance = false;
    [SerializeField] private float targetDistance = 3;
    [SerializeField] private float targetFrequncy = 1;

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;
    [SerializeField] private SpriteRenderer gunSprite;
    private bool objetc = false;
    private GameObject grappeledObject;
    private bool girado;
    [HideInInspector] public bool inCoolDown;
    private bool ground;
    [SerializeField] private float groundSubida=2.3f;
    IEnumerator solt;

    [Header("Enganchar enemigos (piratas y cangrejos):")]
    [Tooltip("Velocidad (unidades por segundo) a la que el gancho atrae al enemigo hacia Izzy. Las cajas van a unas 3-4 u/s y frenan al acercarse")]
    [Range(1f, 25f)][SerializeField] private float velocidadAtraerEnemigo = 8f;
    [Tooltip("Separacion entre el cuerpo del enemigo y el de Izzy a la que se detiene (cuerpo a cuerpo)")]
    [Range(0f, 1f)][SerializeField] private float separacionFinalEnemigo = 0.1f;
    [Tooltip("Segundos que el enemigo queda aturdido. Empiezan a contar cuando ya esta junto a Izzy")]
    [SerializeField] private float tiempoAturdimiento = 0.6f;
    private float ultimaDistanciaEnemigo;     // para detectar si el enemigo se ha quedado atascado
    private float tiempoSinAvanzar;
    private IEnganchable enemigoEnganchado;   // enemigo al que apunta el gancho lanzado
    private Collider2D colliderEnemigo;       // collider tocado, para que la cuerda siga al enemigo
    private bool atrayendoEnemigo;            // el gancho llego al enemigo y lo esta arrastrando

    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip ganchoAudio;


    private void Awake()
    {
        inputs = new Mapa();
        inputs.Enable();
    }

    private void OnDestroy()
    {
        // Liberar las acciones de entrada creadas en Awake (evita fugas al cambiar de escena)
        inputs?.Disable();
        inputs?.Dispose();
    }
    private void Start()
    {
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;
        player.GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        ground = false;


    }


    private void OnEnable()
    {
        //inputs.Player.gancho.performed += OnEngagePerformed;
        //inputs.Player.gancho.canceled += OnEngageCanceled;
        //inputs.Player.gancho.started += OnEngageStarted;
    }
    private void OnDisable()
    {
        //inputs.Player.gancho.performed -= OnEngagePerformed;
        //inputs.Player.gancho.canceled -= OnEngageCanceled;
        //inputs.Player.gancho.started -= OnEngageStarted;
        if (audioSource!=null) { audioSource.Stop(); }
        // Si se cambia de personaje con un enemigo enganchado, se suelta (queda aturdido)
        SoltarEnemigo();

        // Al cambiar de personaje el gancho se desactiva a mitad de lanzar o recoger la cuerda. Si
        // la cuerda se queda "encendida", al volver a Izzy se reactiva, se vuelve a lanzar sola al
        // ultimo punto enganchado y el resorte ancla a Izzy alli. Se apaga todo del todo
        // (la tirolina y las cuerdas tambien ocultan el gancho con Izzy sin gravedad: solo se le
        // devuelve la gravedad si el gancho estaba en uso)
        bool enUso = (grappleRope != null && grappleRope.enabled) || (m_springJoint2D != null && m_springJoint2D.enabled);
        objetc = false;
        grappeledObject = null;
        if (grappleRope != null)
        {
            grappleRope.enabled = false;
        }
        if (m_springJoint2D != null)
        {
            m_springJoint2D.enabled = false;
        }
        if (enUso && m_rigidbody != null)
        {
            m_rigidbody.gravityScale = 1;
        }
    }
    



    private void Update()
    {
        //Debug.Log(ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        if (player.isActive)
        {
            if (!player.isGrounded)
            {
                if (!ground)
                {
                    gunPivot.transform.localPosition = new Vector3(0.18f,groundSubida, 0);
                    ground = true;
                }
                
            }
            else
            {
                if (ground)
                {
                    gunPivot.transform.localPosition = new Vector3(0.18f, 1.82f, 0);
                    ground = false;
                }
            }
            if (!grappleRope.enabled)
            {
                if (Gamepad.current!=null) 
                {
                    MostrarCursor(false);
                    lookPointObject.gameObject.SetActive(true);
                    Vector2 gamepadInput = inputs.Player.Apuntado.ReadValue<Vector2>();
                    Vector3 newPosition = lookPointObject.position + (Vector3)(gamepadInput * Time.deltaTime * rotationSpeed);

                    // restringe newPosition a estar dentro de la circunferencia
                    Vector2 direction = (Vector2)newPosition - (Vector2)gunPivot.position;
                    if (direction.magnitude > maxDistnace)
                    {
                        Vector2 clampedPosition = (Vector2)gunPivot.position + direction.normalized * maxDistnace;
                        newPosition = new Vector3(clampedPosition.x, clampedPosition.y, lookPointObject.position.z);
                    }

                    lookPointObject.position = newPosition;
                    RotateGun(lookPointObject.position, true);
                }
                else
                {
                    MostrarCursor(true);
                    lookPointObject.gameObject.SetActive(false);
                    RotateGun(m_camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()),true);
                }
                
            }
            else
            {
                RotateGun(grapplePoint, false);
            }
            if (grappeledObject != null&&objetc&& launchToPoint && grappleRope.isGrappling)
            {
                // Mover el objeto hacia el personaje
                grappeledObject.transform.position = Vector2.Lerp(grappeledObject.transform.position, gunHolder.position, Time.deltaTime * launchSpeed);
                grapplePoint = grappeledObject.transform.position;
                //grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
            }
            if (atrayendoEnemigo && colliderEnemigo != null)
            {
                // La punta de la cuerda sigue al enemigo mientras se le arrastra
                grapplePoint = colliderEnemigo.bounds.center;
            }

        }
    }

    private void FixedUpdate()
    {
        if (!atrayendoEnemigo) return;

        // El enemigo murio o se destruyo mientras se le arrastraba
        if (!EnemigoValido() || !enemigoEnganchado.PuedeEngancharse)
        {
            SoltarEnemigo();
            RecogerCuerda();
            return;
        }

        Rigidbody2D cuerpo = enemigoEnganchado.Cuerpo;
        // Separacion real entre los cuerpos (no entre los centros): asi se detiene cuerpo a cuerpo
        float separacion = SeparacionCuerpos(cuerpo);

        // Si se queda atascado (p. ej. contra una pared) se le suelta donde esta
        if (ultimaDistanciaEnemigo - separacion > 0.01f)
        {
            tiempoSinAvanzar = 0f;
        }
        else
        {
            tiempoSinAvanzar += Time.fixedDeltaTime;
        }
        ultimaDistanciaEnemigo = separacion;

        // Margen pequeño: el ultimo paso lo deja justo en la separacion final y, por redondeo,
        // puede quedar unas milesimas por encima
        if (separacion <= separacionFinalEnemigo + 0.05f || tiempoSinAvanzar > 0.25f)
        {
            // Ya esta junto a Izzy: aqui empieza el aturdimiento y la cuerda vuelve
            SoltarEnemigo();
            RecogerCuerda();
            return;
        }

        // Velocidad constante hacia Izzy, sin pasarse de la separacion final en este paso
        Vector2 haciaIzzy = ((Vector2)gunHolder.position - cuerpo.position).normalized;
        float paso = velocidadAtraerEnemigo * Time.fixedDeltaTime;
        float avance = Mathf.Min(paso, separacion - separacionFinalEnemigo);
        cuerpo.linearVelocity = haciaIzzy * (avance / Time.fixedDeltaTime);
    }

    // Menor distancia entre los colliders solidos del enemigo y los de Izzy (ignora triggers
    // como los de deteccion de daño). Si no los hay, usa la distancia entre centros
    private float SeparacionCuerpos(Rigidbody2D cuerpoEnemigo)
    {
        float minimo = float.MaxValue;
        int nEnemigo = cuerpoEnemigo.GetAttachedColliders(collidersEnemigo);
        int nIzzy = m_rigidbody.GetAttachedColliders(collidersIzzy);
        for (int i = 0; i < nEnemigo; i++)
        {
            Collider2D a = collidersEnemigo[i];
            if (!a.enabled || a.isTrigger) continue;
            for (int j = 0; j < nIzzy; j++)
            {
                Collider2D b = collidersIzzy[j];
                if (!b.enabled || b.isTrigger) continue;
                ColliderDistance2D d = Physics2D.Distance(a, b);
                if (d.isValid && d.distance < minimo) minimo = d.distance;
            }
        }
        if (minimo == float.MaxValue)
        {
            minimo = Vector2.Distance(cuerpoEnemigo.position, m_rigidbody.position);
        }
        return minimo;
    }
    private readonly Collider2D[] collidersEnemigo = new Collider2D[8];
    private readonly Collider2D[] collidersIzzy = new Collider2D[8];

    // La interfaz no se compara con null como un objeto de Unity destruido: se comprueba el componente
    private bool EnemigoValido()
    {
        return enemigoEnganchado != null && (enemigoEnganchado as UnityEngine.Object) != null;
    }

    // Termina el arrastre del enemigo (si lo hay) y lo deja aturdido
    private void SoltarEnemigo()
    {
        if (EnemigoValido() && atrayendoEnemigo)
        {
            enemigoEnganchado.SoltarEnganche(tiempoAturdimiento);
        }
        atrayendoEnemigo = false;
        enemigoEnganchado = null;
        colliderEnemigo = null;
    }

    // Recoge la cuerda tras atraer a un enemigo, igual que al soltar el boton
    private void RecogerCuerda()
    {
        if (solt != null)
        {
            StopCoroutine(solt);
        }
        Canceled();
    }
    
    public void OnEngagePerformed(InputAction.CallbackContext value)
    {
        if (value.performed) 
        {
            if (player.isActive && !player.muerto && !player.pausePlayer)
            {
                if (launchToPoint && grappleRope.isGrappling && !objetc)
                {
                    if (launchType == LaunchType.Transform_Launch)
                    {
                        Vector2 firePointDistnace = firePoint.position - gunHolder.localPosition;
                        Vector2 targetPos = grapplePoint - firePointDistnace;
                        gunHolder.position = Vector2.Lerp(gunHolder.position, targetPos, Time.deltaTime * launchSpeed);
                    }
                }
            }
        }
    }

    public void OnEngageCanceled(InputAction.CallbackContext value)
    {
        if (value.canceled)
        {
            // Con un enemigo ya enganchado, soltar el boton no corta el arrastre: se le trae
            // hasta Izzy y alli queda aturdido
            if (atrayendoEnemigo) return;
            if (player.isActive && !player.muerto && !player.pausePlayer)
            {
                Canceled();
                if (solt!=null)
                {
                    StopCoroutine(solt);
                }
            }
        }
    }
    public void Canceled()
    {
        //grappleRope.enabled = false;
        objetc = false;
        SoltarEnemigo();

        m_springJoint2D.enabled = false;
        if (grappleRope.enabled && grappleRope.strightLine)
        {
            grappleRope.EnableReverse();
            StartCoroutine(CuerdaCoolDown());
        }
        else
        {
            grappleRope.enabled = false;
        }
        //StartCoroutine(cuerdaReturn());
        m_rigidbody.gravityScale = 1;
        if (audioSource != null) audioSource.Stop();
        Eventos.eve.cambiarBarraCoolDown.Invoke(1);
    }
    IEnumerator CuerdaCoolDown()
    {
        inCoolDown = true;
        float tiempoPasado = 0;

        while (tiempoPasado < coolDownTime)
        {
            tiempoPasado += Time.deltaTime;
            Eventos.eve.cambiarBarraCoolDown.Invoke(tiempoPasado / coolDownTime);
            yield return null;
        }

        inCoolDown = false;
    }

    public void OnEngageStarted(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            if (player.isActive && !player.muerto && !player.pausePlayer&&Time.timeScale!=0)
            {
                if (!inCoolDown)
                {
                    if (grappleRope.isReturning)
                    {
                        grappleRope.enabled = false;
                    }
                    solt = Soltar();
                    StartCoroutine(solt);
                    SetGrapplePoint();
                    if (ganchoAudio != null && audioSource != null && grappleRope.enabled)
                    {
                        audioSource.PlayOneShot(ganchoAudio);
                    }
                }
            }
        }
    }


    // Gira al personaje solo si cambia de lado. Antes se reescribia la rotacion en cada frame
    // y, mirando a la izquierda, Unity lo tomaba como un cambio del transform: eso anulaba la
    // interpolacion del Rigidbody2D y el personaje (y la camara) iban a tirones.
    // Solo cambia la visibilidad del cursor si es distinta: asignarla en cada frame llama al
    // sistema operativo en cada frame
    private void MostrarCursor(bool visible)
    {
        if (Cursor.visible != visible)
        {
            Cursor.visible = visible;
        }
    }

    private void MirarHacia(float anguloY)
    {
        Transform t = player.gameObject.transform;
        if (Mathf.Abs(Mathf.DeltaAngle(t.eulerAngles.y, anguloY)) > 1f)
        {
            t.rotation = Quaternion.Euler(0f, anguloY, 0f);
        }
    }

    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        if (angle>90||angle<-90)
        {
            firePoint.transform.localPosition = new Vector3(0.0349f, -0.0074f, 0);
            gunSprite.flipY = true;
            gunSprite.transform.localRotation = Quaternion.Euler(0, 0, 8);
            MirarHacia(180f);
        }
        else
        {
            firePoint.transform.localPosition = new Vector3(0.0349f, 0.0074f, 0);
            gunSprite.flipY = false;
            gunSprite.transform.localRotation = Quaternion.Euler(0, 0, -8);
            MirarHacia(0f);
        }
        if (rotateOverTime && allowRotationOverTime)
        {
            gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * rotationSpeed);
        }
        else
        {
            gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    private IEnumerator Soltar()
    {
        float tiempoPasado = 0;
        // El gancho dura lo que su sonido; si no tiene sonido asignado, lo que dura el sonido original
        float duracion = ganchoAudio != null ? ganchoAudio.length : 2.33f;

        while (tiempoPasado < duracion)
        {
            tiempoPasado += Time.deltaTime;
            Eventos.eve.cambiarBarraCoolDown.Invoke(1-tiempoPasado/duracion);
            yield return null;
        }
        // Si se acaba el tiempo del gancho arrastrando a un enemigo, se espera a que llegue a
        // Izzy (al llegar se recoge la cuerda y esta rutina se detiene)
        while (atrayendoEnemigo)
        {
            yield return null;
        }
        //yield return new WaitForSeconds(ganchoAudio.length);
        //grappleRope.enabled = false;
        m_springJoint2D.enabled = false;
        grappleRope.EnableReverse();
        inCoolDown = true;
        objetc = false;
        SoltarEnemigo();
        m_rigidbody.gravityScale = 1;
        if (audioSource != null) audioSource.Stop();
        StartCoroutine(CuerdaCoolDown());
    }
    void SetGrapplePoint()
    {
        Vector2 distanceVector;
        if (Gamepad.current != null)
        {
            MostrarCursor(false);
            Vector2 gamepadInput = inputs.Player.Apuntado.ReadValue<Vector2>();
            Vector3 newPosition = lookPointObject.position + (Vector3)(gamepadInput * Time.deltaTime * rotationSpeed);
            distanceVector = (Vector2)newPosition - (Vector2)gunPivot.position;
        }
        else 
        { 
            distanceVector = m_camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - gunPivot.position; 
        }
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, distanceVector.normalized);
        SoltarEnemigo();

        if (hit)
        {
            //Visualizar el raycast en la escena
            Debug.DrawRay(firePoint.position, distanceVector.normalized * hit.distance, Color.green, 0.2f);

            // Piratas y cangrejos (el boss y el fantasma no son enganchables)
            IEnganchable enemigo = hit.collider.GetComponentInParent<IEnganchable>();
            if (enemigo != null && enemigo.PuedeEngancharse)
            {
                if (Vector2.Distance(hit.point, firePoint.position) <= maxDistnace || !hasMaxDistance)
                {
                    grapplePoint = hit.point;
                    grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
                    grappleRope.enabled = true;
                    enemigoEnganchado = enemigo;
                    colliderEnemigo = hit.collider;
                }
                else
                {
                    StopCoroutine(solt);
                }
            }
            else if (hit.transform.gameObject.layer == grappableLayerNumber || grappleToAll)
            {
                if (Vector2.Distance(hit.point, firePoint.position) <= maxDistnace || !hasMaxDistance)
                {
                    grapplePoint = hit.point;
                    grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
                    grappleRope.enabled = true;

                    // Mensaje de depuración para imprimir la información del Raycast
                }
                else
                {
                    StopCoroutine(solt);
                }
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Objetos"))
            {
                if (Vector2.Distance(hit.point, firePoint.position) <= maxDistnace || !hasMaxDistance)
                {
                    grapplePoint = hit.point;
                    grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
                    grappleRope.enabled = true;
                    //objetc = true;
                    grappeledObject = hit.transform.gameObject;
                    objetc = true;
                    // Mensaje de depuración para imprimir la información del Raycast
                }
                else
                {
                    StopCoroutine(solt);
                }
            }
            else
            {
                StopCoroutine(solt);
            }


        }
        else
        {
            StopCoroutine(solt);
        }



    }

    public void Grapple()
    {
        // La cuerda llego a un enemigo: se le arrastra hacia Izzy (Izzy no se mueve)
        if (enemigoEnganchado != null)
        {
            if (EnemigoValido() && enemigoEnganchado.PuedeEngancharse)
            {
                enemigoEnganchado.Enganchar();
                atrayendoEnemigo = true;
                ultimaDistanciaEnemigo = float.MaxValue;
                tiempoSinAvanzar = 0f;
            }
            else
            {
                SoltarEnemigo();
                RecogerCuerda();
            }
            return;
        }

        m_springJoint2D.autoConfigureDistance = false;
        if (!launchToPoint && !autoConfigureDistance)
        {
            m_springJoint2D.distance = targetDistance;
            m_springJoint2D.frequency = targetFrequncy;
        }
        if (!launchToPoint)
        {
            if (autoConfigureDistance)
            {
                m_springJoint2D.autoConfigureDistance = true;
                m_springJoint2D.frequency = 0;
            }

            m_springJoint2D.connectedAnchor = grapplePoint;
            m_springJoint2D.enabled = true;
        }
        else
        {
            switch (launchType)
            {
                case LaunchType.Physics_Launch:
                    m_springJoint2D.connectedAnchor = grapplePoint;

                    Vector2 distanceVector = firePoint.position - gunHolder.position;

                    m_springJoint2D.distance = distanceVector.magnitude;
                    m_springJoint2D.frequency = launchSpeed;
                    m_springJoint2D.enabled = true;
                    break;
                case LaunchType.Transform_Launch:
                    m_rigidbody.gravityScale = 0;
                    m_rigidbody.linearVelocity = Vector2.zero;
                    break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null && hasMaxDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistnace);
        }
    }

}
