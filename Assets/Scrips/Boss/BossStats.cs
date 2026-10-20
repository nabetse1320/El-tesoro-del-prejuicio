using UnityEngine;
using UnityEngine.Events;


public class BossStats : MonoBehaviour
{
    public bool isIdle;
    public bool isStunned;
    public bool isJumping;

    [SerializeField] private float _MaxHealth;
    [SerializeField] private float _CurrentHealth;
    [SerializeField] private float _Damage = 1;
    public UnityEvent eventsToStuned;
    public float attackRange;
    [Tooltip("Diferencia de altura maxima con el jugador para poder golpearlo")]
    public float attackHeight = 2.5f;
    [Tooltip("Tiempo entre ataques")]
    public float attackCooldown = 1.5f;
    [Tooltip("Altura del salto del boss")]
    public float jumpForce;
    public float jumpDetectDistance; // Distancia para detectar si el jugador está encima
    public float speed;
    public float stunDuration; //tiempo que pasa Stuneado el enemigo
    public Transform LeftCorner;
    public Transform RightCorner;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private UnityEvent eventsToDead;

    private bool isDead;

    public bool IsDead { get { return isDead; } }
    public float Damage { get { return _Damage; } }

    private void Start()
    {
        _CurrentHealth = _MaxHealth;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (isStunned)
        {
            _CurrentHealth -= damage;
            if (_CurrentHealth <= 0)
            {
                Morir();
            }
        }
    }

    private void Morir()
    {
        // Solo se ejecuta una vez, aunque reciba mas golpes en el mismo frame
        isDead = true;
        eventsToDead.Invoke();
        Invoke("Destroy", 0.5f);
    }

    private void Destroy()
    {
        Destroy(this.gameObject);
    }

}
