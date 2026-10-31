using UnityEngine;

// Cuerda colgante para trepar (Izzy y Liam). Mientras se mantiene la tecla de interactuar, el
// personaje pone la mano en la cuerda y sube a velocidad constante con la animacion de trepar.
// Al soltar la tecla cae. Pulsando izquierda o derecha salta fuera de la cuerda hacia ese lado.
// Al llegar arriba se sube al suelo de encima si lo hay (dentro de los limites de la camara);
// si no, se queda colgado arriba.
public class Liana : MonoBehaviour, IInteractable
{
    [Tooltip("Donde termina la subida: el personaje se suelta cuando su centro llega a esta altura")]
    [SerializeField] private Transform EndPoint;
    [Tooltip("Margen en Y con el que se considera que ha llegado arriba")]
    [SerializeField] private float threshold = 0.5f;

    [Header("Movimiento:")]
    [Tooltip("Velocidad de subida (unidades por segundo)")]
    [SerializeField] private float velocidadSubida = 3.5f;
    [Tooltip("Segundos que tarda en llevar la mano a la cuerda al agarrarse")]
    [SerializeField] private float tiempoAgarre = 0.12f;
    [Tooltip("Velocidad hacia arriba al saltar fuera de la cuerda con izquierda o derecha")]
    [SerializeField] private float impulsoArriba = 5f;
    [Tooltip("Velocidad hacia el lado al saltar fuera de la cuerda con izquierda o derecha")]
    [SerializeField] private float impulsoLateral = 4f;
    [Tooltip("Al llegar arriba, busca suelo hasta esta altura sobre la punta de la cuerda y sube a el")]
    [SerializeField] private float alturaBorde = 2.5f;
    [Tooltip("Segundos que tarda en subirse al suelo de arriba")]
    [SerializeField] private float tiempoSubirBorde = 0.3f;

    [Header("Agarre:")]
    [Tooltip("Mueve a la derecha (+) o izquierda (-) el punto de agarre respecto al centro del collider de la cuerda, por si el dibujo no coincide")]
    [SerializeField] private float ajusteCuerda = 0f;

    private BoxCollider2D cuerda;
    private AgarreJugador agarre;

    // Cuerda como segmento de 'abajo' a 'abajo + Vector2.up * largo' (sigue su inclinacion)
    private Vector2 abajo;
    private Vector2 direccion;
    private float largo;

    private float posicion;       // distancia desde 'abajo' donde esta la mano
    private float tiempo;
    private float duracionAgarre;
    private Vector2 posicionAlAgarrar;

    // Subida al suelo de arriba al terminar la cuerda
    private bool subiendoBorde;
    private bool enLaCima;          // llego arriba y no habia suelo al que subir
    private bool direccionSoltada;  // se solto izquierda/derecha desde que se agarro
    private Vector2 inicioBorde, destinoBorde;
    private float tiempoBorde;
    private readonly RaycastHit2D[] impactos = new RaycastHit2D[8];
    private readonly Collider2D[] solapes = new Collider2D[8];

    private void Awake()
    {
        cuerda = GetComponent<BoxCollider2D>();
    }

    private void CalcularCuerda()
    {
        Vector2 centro = cuerda.offset;
        float mitad = cuerda.size.x * 0.5f;
        Vector2 a = transform.TransformPoint(centro - new Vector2(mitad, 0f));
        Vector2 b = transform.TransformPoint(centro + new Vector2(mitad, 0f));
        a.x += ajusteCuerda;
        b.x += ajusteCuerda;
        // El extremo de abajo es el de menor altura
        if (a.y > b.y)
        {
            Vector2 c = a; a = b; b = c;
        }
        abajo = a;
        largo = Vector2.Distance(a, b);
        direccion = largo > 0.0001f ? (b - a) / largo : Vector2.up;
    }

    private float Proyectar(Vector2 punto)
    {
        return Mathf.Clamp(Vector2.Dot(punto - abajo, direccion), 0f, largo);
    }

    private Vector2 Punto(float distancia)
    {
        return abajo + direccion * distancia;
    }

    public void Interact()
    {
        if (agarre != null || cuerda == null || EndPoint == null) return;

        GameObject jugador = Interactor.JugadorInteractuando != null
            ? Interactor.JugadorInteractuando
            : GameObject.FindGameObjectWithTag("Player");
        AgarreJugador nuevo = AgarreJugador.Crear(jugador, AgarreJugador.Pose.Trepar);
        if (nuevo == null || !nuevo.PuedeAgarrarse) return;
        if (nuevo.Cuerpo.position.y >= EndPoint.position.y - threshold) return; // ya esta arriba

        CalcularCuerda();
        posicion = Proyectar(nuevo.Mano);

        agarre = nuevo;
        tiempo = 0f;
        enLaCima = false;
        direccionSoltada = false;
        posicionAlAgarrar = agarre.Cuerpo.position;
        float distanciaAgarre = Vector2.Distance(posicionAlAgarrar, agarre.PosicionParaMano(Punto(posicion)));
        duracionAgarre = tiempoAgarre + distanciaAgarre / 12f;
        agarre.Empezar();
        // Quieto mientras pone la mano en la cuerda
        if (agarre.Animador != null) agarre.Animador.speed = 0f;
    }

    private void Update()
    {
        if (agarre == null || !subiendoBorde) return;
        if (agarre.DebeSoltarse)
        {
            Soltar(Vector2.zero);
            return;
        }
        // Se mueve el transform (con la fisica del personaje apagada) para atravesar el borde
        tiempoBorde += Time.deltaTime;
        float k = Mathf.SmoothStep(0f, 1f, tiempoBorde / tiempoSubirBorde);
        agarre.Jugador.transform.position = Vector2.Lerp(inicioBorde, destinoBorde, k);
        if (tiempoBorde >= tiempoSubirBorde)
        {
            Soltar(Vector2.zero);
        }
    }

    private void FixedUpdate()
    {
        if (agarre == null || subiendoBorde) return;
        if (agarre.DebeSoltarse)
        {
            Soltar(Vector2.zero);
            return;
        }

        float dt = Time.fixedDeltaTime;
        tiempo += dt;
        bool arriba = false;

        // Izquierda o derecha: salta fuera de la cuerda hacia ese lado (para bajarse a un suelo)
        // Solo cuenta despues de haber soltado la direccion (si se llego a la cuerda corriendo con
        // la flecha pulsada no salta nada mas agarrarse)
        float lado = AgarreJugador.EntradaMovimiento.x;
        if (Mathf.Abs(lado) < 0.3f) direccionSoltada = true;
        if (tiempo >= duracionAgarre && direccionSoltada && Mathf.Abs(lado) > 0.5f)
        {
            agarre.MirarHacia(lado);
            Soltar(new Vector2(Mathf.Sign(lado) * impulsoLateral, impulsoArriba));
            return;
        }

        if (tiempo >= duracionAgarre && !enLaCima)
        {
            if (agarre.Animador != null) agarre.Animador.speed = 1f;
            posicion += velocidadSubida * dt;
            if (posicion >= largo)
            {
                posicion = largo;
                arriba = true;
            }
        }

        Vector2 objetivo = agarre.PosicionParaMano(Punto(posicion));
        if (tiempo < duracionAgarre)
        {
            float k = Mathf.SmoothStep(0f, 1f, tiempo / duracionAgarre);
            objetivo = Vector2.Lerp(posicionAlAgarrar, objetivo, k);
        }
        agarre.Cuerpo.linearVelocity = Vector2.zero;
        agarre.Cuerpo.MovePosition(objetivo);

        if (!enLaCima && objetivo.y >= EndPoint.position.y - threshold)
        {
            arriba = true;
        }
        if (arriba)
        {
            Vector2 destino;
            if (BuscarSueloArriba(Punto(largo), out destino))
            {
                // Se sube al suelo que hay sobre la cuerda
                subiendoBorde = true;
                tiempoBorde = 0f;
                inicioBorde = agarre.Cuerpo.position;
                destinoBorde = destino;
                agarre.Cuerpo.simulated = false;
            }
            else
            {
                // Nada a lo que subir: se queda colgado arriba hasta soltar la tecla o saltar a un lado
                enLaCima = true;
                if (agarre.Animador != null) agarre.Animador.speed = 0f;
            }
        }
    }

    // Busca un suelo sobre la punta de la cuerda donde quepa el personaje de pie. Prueba encima
    // de la cuerda y, si no cabe, un poco hacia delante y hacia atras
    private bool BuscarSueloArriba(Vector2 cima, out Vector2 posicion)
    {
        posicion = Vector2.zero;
        Rigidbody2D cuerpo = agarre.Cuerpo;

        // Collider solido principal del personaje: su tamaño y donde quedan los pies respecto al centro
        Collider2D principal = null;
        foreach (Collider2D c in cuerpo.GetComponents<Collider2D>())
        {
            if (c.enabled && !c.isTrigger) { principal = c; break; }
        }
        if (principal == null) return false;
        Bounds b = principal.bounds;
        float pies = cuerpo.position.y - b.min.y;
        Vector2 centroRelativo = (Vector2)b.center - cuerpo.position;
        Vector2 tamaño = (Vector2)b.size * 0.95f;

        float delante = agarre.Jugador.transform.eulerAngles.y > 90f ? -1f : 1f;
        float[] desplazamientos = { 0f, 0.6f * delante, -0.6f * delante, 1.2f * delante, -1.2f * delante };
        foreach (float dx in desplazamientos)
        {
            Vector2 origen = new Vector2(cima.x + dx, cima.y + alturaBorde);
            int n = Physics2D.RaycastNonAlloc(origen, Vector2.down, impactos, alturaBorde + 0.5f);
            for (int i = 0; i < n; i++)
            {
                Collider2D c = impactos[i].collider;
                if (c.isTrigger || c.attachedRigidbody == cuerpo) continue;
                Vector2 candidata = new Vector2(origen.x, impactos[i].point.y + pies + 0.02f);
                Vector2 centroDestino = candidata + centroRelativo;
                if (DentroDelMapa(centroDestino) && Cabe(centroDestino, tamaño, cuerpo)
                    && CaminoLibre((Vector2)b.center, centroDestino, tamaño * 0.9f, cuerpo))
                {
                    posicion = candidata;
                    return true;
                }
                break; // el primer suelo solido es el que cuenta en esta columna
            }
        }
        return false;
    }

    // El destino debe estar dentro de alguna zona de la camara (CinemachineConfiner2D). Asi no se
    // sube "encima" del techo que hace de borde del mapa. Sin zonas de camara no se limita
    private static bool DentroDelMapa(Vector2 punto)
    {
        bool hayZonas = false;
        foreach (Cinemachine.CinemachineConfiner2D confiner in FindObjectsOfType<Cinemachine.CinemachineConfiner2D>())
        {
            Collider2D zona = confiner.m_BoundingShape2D;
            if (zona == null) continue;
            hayZonas = true;
            if (zona.OverlapPoint(punto)) return true;
        }
        return !hayZonas;
    }

    // Comprueba que se puede llegar al destino sin atravesar nada solido: primero subiendo en
    // vertical hasta su altura y luego de lado. Asi no se cruza el suelo del que cuelga la cuerda
    private bool CaminoLibre(Vector2 desde, Vector2 hasta, Vector2 tamaño, Rigidbody2D cuerpo)
    {
        Vector2 esquina = new Vector2(desde.x, Mathf.Max(desde.y, hasta.y));
        return TramoLibre(desde, esquina, tamaño, cuerpo) && TramoLibre(esquina, hasta, tamaño, cuerpo);
    }

    private bool TramoLibre(Vector2 desde, Vector2 hasta, Vector2 tamaño, Rigidbody2D cuerpo)
    {
        Vector2 recorrido = hasta - desde;
        float distancia = recorrido.magnitude;
        if (distancia < 0.01f) return true;
        int n = Physics2D.BoxCastNonAlloc(desde, tamaño, 0f, recorrido / distancia, impactos, distancia);
        for (int i = 0; i < n; i++)
        {
            Collider2D c = impactos[i].collider;
            if (!c.isTrigger && c.attachedRigidbody != cuerpo) return false;
        }
        return true;
    }

    private bool Cabe(Vector2 centro, Vector2 tamaño, Rigidbody2D cuerpo)
    {
        int n = Physics2D.OverlapBoxNonAlloc(centro, tamaño, 0f, solapes);
        for (int i = 0; i < n; i++)
        {
            if (!solapes[i].isTrigger && solapes[i].attachedRigidbody != cuerpo) return false;
        }
        return true;
    }

    public void EndInteract()
    {
        // Si ya se esta subiendo al borde, se termina la subida aunque se suelte la tecla
        if (agarre != null && !subiendoBorde)
        {
            Soltar(Vector2.zero);
        }
    }

    private void Soltar(Vector2 velocidadSalida)
    {
        AgarreJugador anterior = agarre;
        agarre = null;
        if (subiendoBorde)
        {
            subiendoBorde = false;
            if (anterior.Cuerpo != null)
            {
                anterior.Cuerpo.simulated = true;
                anterior.Cuerpo.position = anterior.Jugador.transform.position;
            }
        }
        anterior.Terminar(velocidadSalida);
    }

    private void OnDisable()
    {
        if (agarre != null)
        {
            Soltar(Vector2.zero);
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D c = GetComponent<BoxCollider2D>();
        if (c == null) return;
        cuerda = c;
        CalcularCuerda();
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(abajo, Punto(largo));
        if (EndPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(EndPoint.position + Vector3.left * 0.5f, EndPoint.position + Vector3.right * 0.5f);
        }
    }
}
