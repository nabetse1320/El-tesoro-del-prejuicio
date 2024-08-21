
using UnityEngine;
using UnityEngine.Events;


public class BossStats : MonoBehaviour
{
    public bool isIdle;
    public bool isStunned;
    public bool isJumping;
    
    [SerializeField] private float _MaxHealth;
    [SerializeField] private float _CurrentHealth;
    [SerializeField] private float _Damage;
    public UnityEvent eventsToStuned;
    public float attackRange;
    public float jumpForce; 
    public float jumpDetectDistance; // Distancia para detectar si el jugador está encima
    public float speed;
    public float stunDuration; //tiempo que pasa Stuneado el enemigo
    public Transform LeftCorner;
    public Transform RightCorner;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private UnityEvent eventsToDead;

    private void Update()
    {
        if (_CurrentHealth <=0)
        {
            eventsToDead.Invoke();
            Invoke("Destroy",0.5f);
        }
    }
    private void Start()
    {
        _CurrentHealth = _MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isStunned) 
        {
            _CurrentHealth -= damage;
        }
        
    }
    private void Destroy()
    {
        Destroy(this.gameObject);
    }
    
}

