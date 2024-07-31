
using UnityEngine;

public class BossController : MonoBehaviour
{
    private BossStats _stats;
    private GameObject _objetive;
    [SerializeField] private string _BoxesTag;
    private float journeyLength;
    private float startTime;
    private float stunTimer;

    private void Awake()
    {
        _stats = gameObject.GetComponent<BossStats>();
        _objetive = GameObject.FindGameObjectWithTag("Player");
    }
    
    void Start()
    {
        // Calcular la distancia entre los puntos A y B
        journeyLength = Vector3.Distance(_stats.LeftCorner.position, _stats.RightCorner.position);
        startTime = Time.time;
    }

    void Update()
    {
        GroundChecker();
        if(_stats.isIdle) return;
        if (_stats.isStunned)
        {
            // Si está stunado, contar el tiempo de stun
            stunTimer += Time.deltaTime;
            if (stunTimer >= _stats.stunDuration)
            {
                // Terminó el stun, restablecer el estado
                _stats.isStunned = false;
                stunTimer = 0.0f;
            }
        }
        else
        {
            if (!_stats.isJumping)
            {
                Move();
            }
            FindPlayer();
        }
    }

    private void Move()
    {
        // Calcular el tiempo que ha pasado desde el inicio
        float distCovered = (Time.time - startTime) * _stats.speed;

        // Calcular la fracción del camino completado
        float fracJourney = distCovered / journeyLength;

        // Mover el enemigo a lo largo del camino con Lerp
        transform.position = Vector3.Lerp(_stats.LeftCorner.position, _stats.RightCorner.position, Mathf.PingPong(fracJourney, 1));
        
        
    }

    void FindPlayer()
    {
        // Verificar la distancia al jugador
        float distanceToPlayer = Vector3.Distance(transform.position, _objetive.transform.position);

        // Si el jugador está dentro del rango de ataque
        if (distanceToPlayer <= _stats.attackRange)
        {
            // Aquí puedes agregar la lógica para atacar al jugador
            Attack();
        }

        // Verificar si el jugador está encima del enemigo
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, _stats.jumpDetectDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Saltar y luego atacar al jugador
                JumpAndAttack();
            }
        }
    }

    void Attack()
    {
        // Implementa aquí la lógica para atacar al jugador
        Debug.Log("Atacando al jugador!");
        // Usar _stats.Damage para calcular el daño a hacer al jugador
    }
    
    void JumpAndAttack()
    {
        if (!_stats.isJumping)
        {
            // Aplicar una fuerza hacia arriba para simular el salto
            GetComponent<Rigidbody>().AddForce(Vector3.up * _stats.jumpForce, ForceMode.Impulse);
            _stats.isJumping = true;

            // Luego de un tiempo, atacar al jugador (puedes ajustar el tiempo según tus necesidades)
            Invoke("AttackPlayer", 1.0f);
        }
    }

    public void DestroyBoxes() //funcion a activar con el interruptor al desactivarse, tiene que ser antes de que se instancien las nuevas cajas
    
    {
        GameObject[] Boxes = GameObject.FindGameObjectsWithTag(_BoxesTag);
        foreach (GameObject box in Boxes)
        {
            Destroy(box);
        }
    }

    private void GroundChecker()
    {
        // Detectar si el enemigo ha tocado el suelo para restablecer isJumping
        RaycastHit groundHit;
        if (!_stats.isJumping)
        {
            // Si no está saltando actualmente, verificar si ha tocado el suelo
            if (Physics.Raycast(transform.position, Vector3.down, out groundHit, 1.0f)) // Raycast hacia abajo con una distancia corta
            {
                // Si el Raycast detecta una superficie, restablecer isJumping a false
                _stats.isJumping = groundHit.collider != null;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si la colisión es con un objeto con tag "caja" y viene desde arriba
        if (!_stats.isStunned && collision.gameObject.CompareTag(_BoxesTag) && collision.contacts[0].normal.y > 0.7f)
        {
            // Stunear al enemigo
            _stats.isStunned = true;
            Debug.Log("¡El enemigo ha sido stunado!");

            // Aquí podrías desactivar el movimiento del enemigo, reproducir una animación de stun, etc.

            // Iniciar el temporizador de stun
            stunTimer = 0.0f;
        }
    }

}
