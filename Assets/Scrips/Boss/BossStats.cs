
using UnityEngine;


public class BossStats : MonoBehaviour
{
    public bool isIdle;
    public bool isStunned;
    public bool isJumping;
    
    [SerializeField] private float _MaxHealth;
    private float _CurrentHealth;
    [SerializeField] private float _Damage;
    public float attackRange;
    public float jumpForce; 
    public float jumpDetectDistance; // Distancia para detectar si el jugador está encima
    public float speed;
    public float stunDuration; //tiempo que pasa Stuneado el enemigo
    public Transform LeftCorner;
    public Transform RightCorner;
    [SerializeField] private Rigidbody2D rb;

    private void Start()
    {
        _CurrentHealth = _MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        _CurrentHealth -= damage;
    }
    
}

