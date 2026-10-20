using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private BossStats _stats;
    private GameObject _objetive;
    private Rigidbody2D _rb;
    private Collider2D _collider;
    [SerializeField] private string _BoxesTag;
    [SerializeField] private Animator _animator;
    [Tooltip("Marcar si el sprite original del boss mira hacia la derecha")]
    [SerializeField] private bool spriteMiraALaDerecha = true;
    [Tooltip("Duracion total del salto (subida + bajada)")]
    [SerializeField] private float duracionSalto = 1.0f;
    [Tooltip("Tiempo que espera tras aterrizar antes de poder volver a saltar")]
    [SerializeField] private float cooldownSalto = 2.0f;

    private float stunTimer;
    private float attackTimer;
    private float jumpTimer;
    private bool atacando = false;
    private int direccion = 1; // 1 derecha, -1 izquierda
    private float baseY;
    private SpriteRenderer _sprite;
    private bool muerto;

    private void Awake()
    {
        _stats = gameObject.GetComponent<BossStats>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        if (_animator != null)
        {
            _sprite = _animator.GetComponent<SpriteRenderer>();
        }
    }

    void Start()
    {
        // El boss se mueve por script, asi que el Rigidbody no debe aplicarle gravedad
        // (su collider es trigger y si cayera atravesaria el piso)
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.velocity = Vector2.zero;
        }
        baseY = _stats.LeftCorner.position.y;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, MinX(), MaxX()), baseY, transform.position.z);
        _stats.isJumping = false;
    }

    void Update()
    {
        if (_stats.IsDead)
        {
            if (!muerto)
            {
                muerto = true;
                StopAllCoroutines();
                SetAnimBool("Atacando", false);
                SetAnimBool("Muerto", true);
            }
            return;
        }

        BuscarObjetivo();
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (jumpTimer > 0) jumpTimer -= Time.deltaTime;

        if (_stats.isIdle) return;

        if (_stats.isStunned)
        {
            SetAnimBool("idle", true);
            // Si está stunado, contar el tiempo de stun
            stunTimer += Time.deltaTime;
            if (stunTimer >= _stats.stunDuration)
            {
                // Terminó el stun, restablecer el estado
                _stats.isStunned = false;
                stunTimer = 0.0f;
                SetAnimBool("idle", false);
            }
            return;
        }

        if (!_stats.isJumping && !atacando)
        {
            Move();
        }
        FindPlayer();
    }

    private float MinX()
    {
        return Mathf.Min(_stats.LeftCorner.position.x, _stats.RightCorner.position.x);
    }

    private float MaxX()
    {
        return Mathf.Max(_stats.LeftCorner.position.x, _stats.RightCorner.position.x);
    }

    private void Move()
    {
        // Patrulla entre las dos esquinas partiendo de la posicion actual,
        // asi al terminar un stun, salto o ataque continua desde donde quedo
        float destino = direccion > 0 ? MaxX() : MinX();
        float nuevaX = Mathf.MoveTowards(transform.position.x, destino, _stats.speed * Time.deltaTime);
        transform.position = new Vector3(nuevaX, baseY, transform.position.z);

        if (Mathf.Approximately(nuevaX, destino))
        {
            direccion = -direccion;
        }
        Mirar(direccion);
    }

    private void Mirar(int dir)
    {
        if (_sprite == null) return;
        bool mirandoDerecha = dir > 0;
        _sprite.flipX = spriteMiraALaDerecha ? !mirandoDerecha : mirandoDerecha;
    }

    private void BuscarObjetivo()
    {
        // El tag "Player" lo tiene el personaje activo y cambia al hacer switch
        if (_objetive == null || !_objetive.CompareTag("Player") || !_objetive.activeInHierarchy)
        {
            _objetive = GameObject.FindGameObjectWithTag("Player");
        }
    }

    void FindPlayer()
    {
        if (_objetive == null) return;

        Vector2 distancia = _objetive.transform.position - transform.position;

        // Si el jugador está dentro del rango de ataque y a una altura alcanzable
        if (!atacando && !_stats.isJumping && attackTimer <= 0 && JugadorEnRango())
        {
            Mirar(distancia.x >= 0 ? 1 : -1);
            StartCoroutine(Ataque());
            return;
        }

        // Verificar si el jugador está encima del enemigo
        if (!atacando && !_stats.isJumping && jumpTimer <= 0 && JugadorEncima())
        {
            StartCoroutine(JumpAndAttack());
        }
    }

    private bool JugadorEnRango()
    {
        Vector2 distancia = _objetive.transform.position - transform.position;
        return Mathf.Abs(distancia.x) <= _stats.attackRange && Mathf.Abs(distancia.y) <= _stats.attackHeight;
    }

    private bool JugadorEncima()
    {
        Vector2 origen = transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origen, Vector2.up, _stats.jumpDetectDistance);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider == _collider) continue;
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    void Attack()
    {
        // Cada invocacion de perderVida quita un punto de vida al jugador
        int golpes = Mathf.Max(1, Mathf.RoundToInt(_stats.Damage));
        for (int i = 0; i < golpes; i++)
        {
            Eventos.eve.perderVida.Invoke();
        }
    }

    IEnumerator Ataque()
    {
        SetAnimBool("Atacando", true);
        atacando = true;
        yield return new WaitForSeconds(0.5f);
        // Solo hace daño si el jugador no se alejó durante la animacion
        if (!_stats.isStunned && !_stats.IsDead && _objetive != null && JugadorEnRango())
        {
            Attack();
        }
        attackTimer = _stats.attackCooldown;
        atacando = false;
        SetAnimBool("Atacando", false);
    }

    IEnumerator JumpAndAttack()
    {
        _stats.isJumping = true;
        float tiempo = 0f;
        bool golpeo = false;
        float altura = _stats.jumpForce;

        // Salto en arco controlado por script (el collider es trigger y no puede apoyarse en el piso)
        while (tiempo < duracionSalto)
        {
            if (_stats.isStunned || _stats.IsDead) break;
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracionSalto);
            float y = baseY + altura * 4f * t * (1f - t);
            transform.position = new Vector3(transform.position.x, y, transform.position.z);

            // En el punto mas alto intenta golpear al jugador
            if (!golpeo && t >= 0.5f)
            {
                golpeo = true;
                StartCoroutine(Ataque());
            }
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, baseY, transform.position.z);
        _stats.isJumping = false;
        jumpTimer = cooldownSalto;
    }

    public void DestroyBoxes() //funcion a activar con el interruptor al desactivarse, tiene que ser antes de que se instancien las nuevas cajas

    {
        GameObject[] Boxes = GameObject.FindGameObjectsWithTag(_BoxesTag);
        foreach (GameObject box in Boxes)
        {
            Destroy(box);
        }
    }

    public void Stunear()
    {
        if (_stats.IsDead) return;
        // Stunear al enemigo
        _stats.isStunned = true;
        _stats.eventsToStuned.Invoke();
        Debug.Log("¡El enemigo ha sido stunado!");

        // Interrumpe el ataque o salto en curso y lo devuelve al piso
        StopAllCoroutines();
        atacando = false;
        _stats.isJumping = false;
        SetAnimBool("Atacando", false);
        transform.position = new Vector3(transform.position.x, baseY, transform.position.z);

        // Iniciar el temporizador de stun
        stunTimer = 0.0f;
    }

    private void SetAnimBool(string parametro, bool valor)
    {
        if (_animator == null) return;
        foreach (AnimatorControllerParameter p in _animator.parameters)
        {
            if (p.name == parametro)
            {
                _animator.SetBool(parametro, valor);
                return;
            }
        }
    }

}
