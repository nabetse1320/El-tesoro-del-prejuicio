using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Intro del combate final: al empezar el nivel, Leonardo "Quimera" aparece sobre la plataforma
// de inicio, habla con los heroes y salta hacia la arena. Al aterrizar empieza el combate.
public class IntroBoss : MonoBehaviour
{
    [Header("Dialogo")]
    [Tooltip("Lineas del dialogo: Leonardo, Izzy o Liam")]
    [SerializeField] private UtilDialogos.Linea[] dialogo = new UtilDialogos.Linea[]
    {
        new UtilDialogos.Linea { personaje = "Leonardo", texto = "Finalmente pude dar con ustedes." },
        new UtilDialogos.Linea { personaje = "Izzy", texto = "¡Calla! Que nosotros llegamos a ti." },
        new UtilDialogos.Linea { personaje = "Leonardo", texto = "Ja, ja, ja, eso quisieran. La verdad, piratas más de mi tamaño, pero no perdono a nadie." },
        new UtilDialogos.Linea { personaje = "Liam", texto = "No lo permitiremos." },
        new UtilDialogos.Linea { personaje = "Leonardo", texto = "Tanto esfuerzo para nada, mi leyenda como el pirata que jamás ha sido derrotado." },
        new UtilDialogos.Linea { personaje = "Izzy", texto = "¡No te saldrás con la tuya! Y no someterás más a los seres de este lugar." },
        new UtilDialogos.Linea { personaje = "Leonardo", texto = "Por favor, fueron hechos para obedecerme, ¡para obedecernos! ¡Ciudad Perdida no ha sido más que una cuna de esclavos!" },
        new UtilDialogos.Linea { personaje = "Leonardo", texto = "Y lo seguirá siendo, incluso cuando yo muera." },
    };
    [SerializeField] private GameObject canvasDialogosPrefab;
    [SerializeField] private float tiempoEntreChar = 0.03f;
    [Tooltip("Retrato de Leonardo (se muestra en el lugar de Malo1)")]
    [SerializeField] private Sprite retratoLeonardo;

    [Header("Salto de entrada")]
    [Tooltip("Posicion inicial de la raiz del boss: la X sobre la plataforma de inicio y la Y de sus pies (12.231) + 1.1796")]
    [SerializeField] private Vector2 posicionInicialBoss = new Vector2(-17f, 13.4106f);
    [Tooltip("X dentro de la arena donde cae el boss")]
    [SerializeField] private float xAterrizaje = -9f;
    [Tooltip("Cuanto sube el boss respecto a su posicion inicial antes de caer")]
    [SerializeField] private float alturaExtraSalto = 3f;
    [Tooltip("Duracion del salto desde que despega hasta que toca el piso de la arena")]
    [SerializeField] private float duracionSaltoIntro = 1.4f;
    [Tooltip("Segundos que los heroes se mueven libremente antes del dialogo")]
    [SerializeField] private float esperaInicial = 0.6f;
    [Tooltip("Tiempo agachado antes de despegar")]
    [SerializeField] private float tiempoDespegue = 0.29f;
    [Tooltip("Duracion de la animacion de aterrizaje")]
    [SerializeField] private float duracionAterrizaje = 0.42f;

    private BossController boss;
    private BossStats bossStats;
    private PlayerSwitch playerSwitch;

    private void Start()
    {
        boss = FindFirstObjectByType<BossController>();
        if (boss == null) return;
        bossStats = boss.GetComponent<BossStats>();
        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        // El boss aparece sobre la plataforma de inicio, mirando hacia los heroes
        Vector3 inicio = new Vector3(posicionInicialBoss.x, posicionInicialBoss.y, boss.transform.position.z);
        boss.transform.position = inicio;
        boss.MirarHacia(-1);
        boss.ReproducirEstado("Idle");

        // Los heroes llegan a la plataforma y despues quedan quietos
        yield return new WaitForSeconds(esperaInicial);
        playerSwitch = FindFirstObjectByType<PlayerSwitch>();
        if (playerSwitch != null)
        {
            float espera = 0f;
            while (espera < 1.5f && MovimientoVerticalHeroes() > 0.05f)
            {
                espera += Time.deltaTime;
                yield return null;
            }
        }

        Eventos.eve.CancelSwitches.Invoke();
        Eventos.eve.PausarPlayer.Invoke();
        Eventos.eve.PausarPlayer2.Invoke();
        if (playerSwitch != null)
        {
            DetenerHeroes();
        }

        yield return StartCoroutine(UtilDialogos.Mostrar(canvasDialogosPrefab, dialogo, tiempoEntreChar, NombreRetrato, PrepararRetratoLeonardo));

        // Salto hacia la arena: primero se agacha y despega, mirando hacia donde cae
        Vector3 inicioSalto = boss.transform.position;
        boss.MirarHacia(xAterrizaje >= inicioSalto.x ? 1 : -1);
        boss.ReproducirEstado("Salto");
        yield return new WaitForSeconds(tiempoDespegue);

        // Parabola vertical: y(t) = inicioY + A*t^2 + B*t, con el pico a una altura h sobre el inicio
        // en el instante tPico, y termina en el piso de la arena (ySuelo) cuando t = 1
        float ySuelo = bossStats.LeftCorner.position.y;
        float h = Mathf.Max(alturaExtraSalto, 0.01f);
        float g = Mathf.Max(inicioSalto.y - ySuelo, 0.01f);
        float tPico = h / (h + Mathf.Sqrt(h * h + g * h));
        float A = -h / (tPico * tPico);
        float B = 2f * h / tPico;
        float duracion = Mathf.Max(duracionSaltoIntro, 0.01f);

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float x = Mathf.Lerp(inicioSalto.x, xAterrizaje, t);
            float y = inicioSalto.y + A * t * t + B * t;
            boss.transform.position = new Vector3(x, y, inicioSalto.z);
            yield return null;
        }

        // Aterriza en la arena
        boss.transform.position = new Vector3(xAterrizaje, ySuelo, inicioSalto.z);
        boss.ReproducirEstado("Aterrizaje");
        yield return new WaitForSeconds(duracionAterrizaje);

        // Fin de la intro: los heroes recuperan el control y empieza el combate
        Eventos.eve.DespausarPlayer.Invoke();
        Eventos.eve.DespausarPlayer2.Invoke();
        Eventos.eve.ActivateSwitches.Invoke();
        boss.IniciarCombate();
        Destroy(gameObject);
    }

    // Liam usa su retrato, Izzy el de Lizzy y Leonardo el de Malo1 (con el sprite de Leonardo)
    private static string NombreRetrato(string personaje)
    {
        string nombre = (personaje ?? string.Empty).Trim().ToLower();
        if (nombre == "liam") return "Liam";
        if (nombre == "izzy") return "Lizzy";
        return "Malo1";
    }

    private void PrepararRetratoLeonardo(GameObject canvas)
    {
        if (retratoLeonardo == null) return;
        Transform malo = canvas.transform.Find("Imagenes/Malo1");
        if (malo == null) return;
        Image imagen = malo.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.sprite = retratoLeonardo;
        }
    }

    private float MovimientoVerticalHeroes()
    {
        float total = 0f;
        foreach (GameObject heroe in new GameObject[] { playerSwitch.p1, playerSwitch.p2 })
        {
            Rigidbody2D rb = heroe != null ? heroe.GetComponent<Rigidbody2D>() : null;
            if (rb != null) total += Mathf.Abs(rb.linearVelocity.y);
        }
        return total;
    }

    // Los heroes quedan quietos y sin animacion de caminar durante el dialogo
    private void DetenerHeroes()
    {
        foreach (GameObject heroe in new GameObject[] { playerSwitch.p1, playerSwitch.p2 })
        {
            if (heroe == null) continue;
            Animator animator = heroe.GetComponent<Animator>();
            ApagarBool(animator, "Corriendo");
            ApagarBool(animator, "CorriendoEspalda");
            Rigidbody2D rb = heroe.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }

    private static void ApagarBool(Animator animator, string parametro)
    {
        if (animator == null) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == parametro && p.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(parametro, false);
                return;
            }
        }
    }
}
