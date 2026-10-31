using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaCaida : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float fallTime;
    [SerializeField] private bool playerStatic;
    private Rigidbody2D rb;

    // Estado inicial, para devolverla a su sitio cuando el jugador reaparece en un checkpoint
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private RigidbodyType2D tipoInicial;
    private Coroutine caida;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        tipoInicial = rb.bodyType;
        Eventos.eve.RevivirPlayer.AddListener(Reiniciar);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerStatic)
            {
                Eventos.eve.PausarPlayer.Invoke();
                Eventos.eve.PausarPlayer2.Invoke();
            }
            // Una sola caida aunque se pise varias veces
            if (caida == null && rb.bodyType == tipoInicial)
            {
                caida = StartCoroutine(Fall());
            }
        }
    }
    IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallTime);
        rb.bodyType = RigidbodyType2D.Dynamic;
        caida = null;
    }

    // Al reaparecer el jugador vuelve a su sitio (si no, al caer una necesaria no se podia avanzar)
    private void Reiniciar()
    {
        if (caida != null)
        {
            StopCoroutine(caida);
            caida = null;
        }
        rb.bodyType = tipoInicial;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = posicionInicial;
        rb.rotation = rotacionInicial.eulerAngles.z;
        transform.SetPositionAndRotation(posicionInicial, rotacionInicial);
    }

    // Se suscribe en Start (y no en OnEnable) para que Eventos ya exista; sigue escuchando aunque
    // la plataforma se desactive al caer
    private void OnDestroy()
    {
        if (Eventos.eve != null) Eventos.eve.RevivirPlayer.RemoveListener(Reiniciar);
    }
}
