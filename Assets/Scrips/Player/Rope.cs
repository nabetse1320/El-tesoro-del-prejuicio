using UnityEngine;

// Tirolina (solo Izzy). Mientras se mantiene la tecla de interactuar, Izzy cuelga del cable con
// el puño justo sobre el, en el punto donde se agarro, y se desliza acelerando hacia el punto
// final. Al soltar la tecla o llegar al final se suelta conservando parte de la velocidad.
public class Rope : MonoBehaviour, IInteractable
{
    [Tooltip("Donde termina el recorrido: el centro de Izzy acaba aqui. Si esta mas alla del cable, al terminarlo sigue colgada hasta este punto")]
    [SerializeField] private Transform EndPoint;
    [Tooltip("Si al acabar el cable Izzy esta a menos de esta distancia del punto final, se suelta directamente")]
    [SerializeField] private float threshold = 0.5f;

    [Header("Movimiento:")]
    [Tooltip("Velocidad maxima por el cable (unidades por segundo)")]
    [SerializeField] private float velocidadMaxima = 9f;
    [Tooltip("Aceleracion al deslizarse (unidades por segundo cada segundo)")]
    [SerializeField] private float aceleracion = 12f;
    [Tooltip("Segundos que tarda Izzy en llevar la mano al cable al agarrarse")]
    [SerializeField] private float tiempoAgarre = 0.12f;
    [Tooltip("Parte de la velocidad que conserva Izzy al soltarse (0 = cae en vertical)")]
    [Range(0f, 1f)][SerializeField] private float impulsoAlSoltar = 0.6f;

    [Header("Agarre:")]
    [Tooltip("Sube (+) o baja (-) el punto de agarre respecto al centro del collider del cable, por si el dibujo no coincide")]
    [SerializeField] private float ajusteCable = 0f;

    private BoxCollider2D cable;
    private AudioSource audioSource;
    private AgarreJugador agarre;

    // Cable como segmento de 'inicio' a 'inicio + direccion * largo'
    private Vector2 inicio;
    private Vector2 direccion;
    private float largo;

    private float posicion;       // distancia recorrida desde 'inicio' donde esta la mano
    private float destino;        // posicion en el cable del punto final
    private float sentido;        // +1 hacia el final del segmento, -1 hacia el inicio
    private float velocidad;
    private bool haciaPuntoFinal; // ya salio del cable y va al punto final (si esta mas alla del cable)
    private float tiempoHaciaPuntoFinal;
    private float tiempo;         // desde que se agarro
    private float duracionAgarre;
    private Vector2 posicionAlAgarrar;

    private void Awake()
    {
        cable = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void CalcularCable()
    {
        // Linea central del collider, a lo largo de su eje X local
        Vector2 centro = cable.offset;
        float mitad = cable.size.x * 0.5f;
        Vector2 a = transform.TransformPoint(centro - new Vector2(mitad, 0f));
        Vector2 b = transform.TransformPoint(centro + new Vector2(mitad, 0f));
        a.y += ajusteCable;
        b.y += ajusteCable;
        inicio = a;
        largo = Vector2.Distance(a, b);
        direccion = largo > 0.0001f ? (b - a) / largo : Vector2.right;
    }

    private float Proyectar(Vector2 punto)
    {
        return Mathf.Clamp(Vector2.Dot(punto - inicio, direccion), 0f, largo);
    }

    private Vector2 Punto(float distancia)
    {
        return inicio + direccion * distancia;
    }

    public void Interact()
    {
        if (agarre != null || cable == null || EndPoint == null) return;

        // Solo Izzy puede usar la tirolina
        GameObject jugador = Interactor.JugadorInteractuando != null
            ? Interactor.JugadorInteractuando
            : GameObject.FindGameObjectWithTag("Player");
        AgarreJugador nuevo = AgarreJugador.Crear(jugador, AgarreJugador.Pose.Tirolina);
        if (nuevo == null || !nuevo.EsIzzy || !nuevo.PuedeAgarrarse) return;

        CalcularCable();
        destino = Proyectar(EndPoint.position);
        float actual = Proyectar(nuevo.Mano);
        if (Mathf.Abs(destino - actual) < 0.3f) return; // ya esta en el final

        sentido = Mathf.Sign(destino - actual);
        // Mira hacia donde va; al girarse cambia la posicion de la mano, por eso se vuelve a proyectar
        nuevo.MirarHacia(direccion.x * sentido);
        posicion = Proyectar(nuevo.Mano);

        agarre = nuevo;
        velocidad = 0f;
        haciaPuntoFinal = false;
        tiempo = 0f;
        posicionAlAgarrar = agarre.Cuerpo.position;
        float distanciaAgarre = Vector2.Distance(posicionAlAgarrar, agarre.PosicionParaMano(Punto(posicion)));
        duracionAgarre = tiempoAgarre + distanciaAgarre / 12f;
        agarre.Empezar();

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void FixedUpdate()
    {
        if (agarre == null) return;
        if (agarre.DebeSoltarse)
        {
            Soltar(false);
            return;
        }

        float dt = Time.fixedDeltaTime;
        tiempo += dt;

        if (haciaPuntoFinal)
        {
            // Fuera del cable: sigue colgada hasta el punto final (normalmente sobre la plataforma
            // de llegada) y alli se suelta, como hacia la tirolina original
            Vector2 actual = agarre.Cuerpo.position;
            Vector2 meta = EndPoint.position;
            Vector2 siguiente = Vector2.MoveTowards(actual, meta, Mathf.Max(velocidad, 3f) * dt);
            agarre.Cuerpo.linearVelocity = Vector2.zero;
            agarre.Cuerpo.MovePosition(siguiente);
            tiempoHaciaPuntoFinal -= dt;
            // Si algo la bloquea y no llega, se suelta igualmente
            if (Vector2.Distance(siguiente, meta) <= 0.05f || tiempoHaciaPuntoFinal <= 0f)
            {
                Soltar(false);
            }
            return;
        }

        bool llego = false;
        if (tiempo >= duracionAgarre)
        {
            velocidad = Mathf.MoveTowards(velocidad, velocidadMaxima, aceleracion * dt);
            posicion += sentido * velocidad * dt;
            if ((sentido > 0f && posicion >= destino) || (sentido < 0f && posicion <= destino))
            {
                posicion = destino;
                llego = true;
            }
        }

        Vector2 objetivo = agarre.PosicionParaMano(Punto(posicion));
        if (tiempo < duracionAgarre)
        {
            // Lleva la mano al cable suavemente
            float k = Mathf.SmoothStep(0f, 1f, tiempo / duracionAgarre);
            objetivo = Vector2.Lerp(posicionAlAgarrar, objetivo, k);
        }
        agarre.Cuerpo.linearVelocity = Vector2.zero;
        agarre.Cuerpo.MovePosition(objetivo);

        if (llego)
        {
            if (Vector2.Distance(objetivo, EndPoint.position) > threshold)
            {
                // El punto final esta mas alla del cable: se termina el recorrido hasta alli
                haciaPuntoFinal = true;
                tiempoHaciaPuntoFinal = Vector2.Distance(objetivo, EndPoint.position) / Mathf.Max(velocidad, 3f) + 0.5f;
            }
            else
            {
                Soltar(true);
            }
        }
    }

    public void EndInteract()
    {
        // Al soltar la tecla se cae, salvo si ya esta llegando al punto final fuera del cable
        if (agarre != null && !haciaPuntoFinal)
        {
            Soltar(true);
        }
    }

    private void Soltar(bool conImpulso)
    {
        Vector2 salida = conImpulso ? direccion * sentido * velocidad * impulsoAlSoltar : Vector2.zero;
        AgarreJugador anterior = agarre;
        agarre = null;
        anterior.Terminar(salida);
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (agarre != null)
        {
            Soltar(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D c = GetComponent<BoxCollider2D>();
        if (c == null) return;
        cable = c;
        CalcularCable();
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(inicio, Punto(largo));
        if (EndPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Punto(Proyectar(EndPoint.position)), 0.2f);
        }
    }
}
