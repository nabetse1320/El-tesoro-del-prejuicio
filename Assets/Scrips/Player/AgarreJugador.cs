using UnityEngine;

// Lo que comparten la tirolina y las cuerdas para trepar: deja al personaje colgado de la mano,
// sin gravedad ni control, con la pose del Animator, y lo devuelve a la normalidad al soltarse.
public class AgarreJugador
{
    public enum Pose { Tirolina, Trepar }

    // Puño de cada pose en unidades del sprite (pivote en el centro, 100 px por unidad), medido en
    // los sprites de Assets/Sprites/<personaje>/tirolina y /trepar. Si se cambian esos sprites hay
    // que volver a medirlo
    private static readonly Vector2 manoIzzyTirolina = new Vector2(-0.155f, 3.0f);
    private static readonly Vector2 manoIzzyTrepar = new Vector2(0.456f, 3.135f);
    private static readonly Vector2 manoLiamTrepar = new Vector2(0.72f, 3.12f);

    public readonly GameObject Jugador;
    public readonly Rigidbody2D Cuerpo;
    public readonly Animator Animador;

    private readonly PlayerController izzy;
    private readonly PlayerController2 liam;
    private readonly GameObject brazoArma;   // brazo con el gancho de Izzy (la pose ya dibuja los brazos)
    private readonly string parametroAnimator;
    private readonly Vector2 manoLocal;      // puño de la pose, en unidades del sprite (pivote en el centro)

    private AgarreJugador(GameObject jugador, Pose pose)
    {
        Jugador = jugador;
        Cuerpo = jugador.GetComponent<Rigidbody2D>();
        Animador = jugador.GetComponent<Animator>();
        izzy = jugador.GetComponent<PlayerController>();
        liam = jugador.GetComponent<PlayerController2>();
        parametroAnimator = pose == Pose.Tirolina ? "Colgado" : "Trepando";
        if (pose == Pose.Tirolina) manoLocal = manoIzzyTirolina; // solo Izzy usa la tirolina
        else manoLocal = izzy != null ? manoIzzyTrepar : manoLiamTrepar;

        GrapplingGun gancho = jugador.GetComponentInChildren<GrapplingGun>(true);
        if (gancho != null && gancho.gunPivot != null)
        {
            brazoArma = gancho.gunPivot.gameObject;
        }
    }

    // Null si el objeto no es Izzy ni Liam
    public static AgarreJugador Crear(GameObject jugador, Pose pose)
    {
        if (jugador == null || jugador.GetComponent<Rigidbody2D>() == null) return null;
        if (jugador.GetComponent<PlayerController>() == null && jugador.GetComponent<PlayerController2>() == null) return null;
        return new AgarreJugador(jugador, pose);
    }

    public bool EsIzzy
    {
        get { return izzy != null; }
    }

    // True si ya no puede seguir agarrado (muerto, desactivado o cambiado de personaje)
    public bool DebeSoltarse
    {
        get
        {
            if (Jugador == null || !Jugador.activeInHierarchy) return true;
            if (izzy != null) return izzy.muerto || !izzy.isActive;
            if (liam != null) return liam.muerto || !liam.isActive;
            return true;
        }
    }

    public bool PuedeAgarrarse
    {
        get { return !DebeSoltarse; }
    }

    // Posicion del puño en el mundo (tiene en cuenta la escala y hacia donde mira)
    public Vector2 Mano
    {
        get { return Cuerpo.position + DesfaseMano(); }
    }

    // Posicion que debe tener el personaje para que su puño quede en 'punto'
    public Vector2 PosicionParaMano(Vector2 punto)
    {
        return punto - DesfaseMano();
    }

    private Vector2 DesfaseMano()
    {
        return Jugador.transform.TransformVector(new Vector3(manoLocal.x, manoLocal.y, 0f));
    }

    // Entrada de movimiento del jugador (el personaje esta en pausa y no la procesa)
    private static UnityEngine.InputSystem.PlayerInput entrada;
    public static Vector2 EntradaMovimiento
    {
        get
        {
            if (entrada == null) entrada = Object.FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
            if (entrada == null || entrada.actions == null) return Vector2.zero;
            UnityEngine.InputSystem.InputAction mover = entrada.actions.FindAction("Move");
            return mover != null && mover.enabled ? mover.ReadValue<Vector2>() : Vector2.zero;
        }
    }

    // Gira al personaje hacia la derecha (dirX > 0) o la izquierda (dirX < 0)
    public void MirarHacia(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;
        float angulo = dirX > 0f ? 0f : 180f;
        if (Mathf.Abs(Mathf.DeltaAngle(Jugador.transform.eulerAngles.y, angulo)) > 1f)
        {
            Jugador.transform.rotation = Quaternion.Euler(0f, angulo, 0f);
        }
    }

    public void Empezar()
    {
        Eventos.eve.CancelSwitches.Invoke();
        Eventos.eve.PausarPlayer.Invoke();
        Eventos.eve.PausarPlayer2.Invoke();
        Cuerpo.gravityScale = 0f;
        Cuerpo.linearVelocity = Vector2.zero;
        if (Animador != null)
        {
            Animador.SetBool(parametroAnimator, true);
        }
        if (brazoArma != null)
        {
            brazoArma.SetActive(false);
        }
    }

    // Velocidad con la que sale el personaje al soltarse
    public void Terminar(Vector2 velocidadSalida)
    {
        if (Cuerpo != null)
        {
            Cuerpo.gravityScale = 1f;
            Cuerpo.linearVelocity = velocidadSalida;
        }
        if (Animador != null)
        {
            Animador.SetBool(parametroAnimator, false);
            Animador.speed = 1f;
        }
        if (izzy != null) izzy.ActualizarPoseAire(velocidadSalida);
        else if (liam != null) liam.ActualizarPoseAire(velocidadSalida);
        if (brazoArma != null)
        {
            brazoArma.SetActive(true);
        }
        Eventos.eve.ActivateSwitches.Invoke();
        // Si se suelta por morir, el control lo devuelve el controlador de vida al reaparecer
        bool muerto = (izzy != null && izzy.muerto) || (liam != null && liam.muerto);
        if (!muerto)
        {
            Eventos.eve.DespausarPlayer.Invoke();
            Eventos.eve.DespausarPlayer2.Invoke();
        }
    }
}
