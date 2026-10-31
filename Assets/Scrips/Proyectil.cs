using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Proyectil : MonoBehaviour
{
    private Vector3 direccion;
    private float velocidadBala;
    

    
    public void SetDireccion(Vector3 dir,float vel)
    {
        direccion = dir;
        velocidadBala = vel;
    }
    public void Destruir()
    {
        Destroy(this.gameObject);
    }

    void Start()
    {
        // Velocidad constante (unidades/segundo): se normaliza la dirección para que
        // la rapidez no dependa de la distancia al jugador ni de la masa del proyectil
        GetComponent<Rigidbody2D>().linearVelocity = direccion.normalized * velocidadBala;
        // Es una bala de cañon redonda con el brillo arriba a la izquierda: va sin girar. Antes
        // usaba la direccion como si fueran angulos y la inclinaba en X (se veia aplastada)
        transform.rotation = Quaternion.identity;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Terreno")||collision.gameObject.CompareTag("ObInteract"))
        {
            Destruir();
        }
    }
}
