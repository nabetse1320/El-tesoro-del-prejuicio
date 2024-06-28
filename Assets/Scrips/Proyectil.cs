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
        GetComponent<Rigidbody2D>().AddForce(direccion* velocidadBala, ForceMode2D.Impulse);
        GetComponent<Transform>().rotation = Quaternion.Euler(direccion.x,0,direccion.z);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Terreno")||collision.gameObject.CompareTag("ObInteract"))
        {
            Destruir();
        }
    }
}
