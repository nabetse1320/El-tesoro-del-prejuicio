using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    public float velocidadSubida = 3f; // Velocidad a la que el jugador sube o baja
    public LayerMask capasEscalera; // Capas que representan las escaleras

    private bool enEscalera;
    [SerializeField] private GameObject player;
    private Rigidbody2D rb;

    private void Start()
    {
        rb= player.GetComponent<Rigidbody2D>();
    }

    public void Interact()
    {
        if (!enEscalera)
        {
            enEscalera = true;
            rb.gravityScale = 0f; // Desactivar la gravedad mientras está en la escalera
        }
        
    }

    public void EndInteract()
    {
        enEscalera = false;
        rb.gravityScale = 1f; // Activar la gravedad nuevamente al finalizar la interacción

    }

    void FixedUpdate()
    {
        if (enEscalera)
        {
            Debug.Log("en escalera");
            // Movimiento vertical mientras está en la escalera
            float inputVertical = player.GetComponent<PlayerController>().moveVector.y;
            rb.velocity = new Vector2(rb.velocity.x, inputVertical * velocidadSubida);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Salir de la escalera si el jugador se aleja
        if (other.CompareTag("Player"))
        {
            EndInteract();
        }
    }
}
