using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaCaida : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float fallTime;
    [SerializeField] private bool playerStatic;
    private Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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
            StartCoroutine(Fall());
        }
    }
    IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallTime);
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
