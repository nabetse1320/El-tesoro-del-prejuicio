using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EjecutarSonido : MonoBehaviour
{
    private AudioSource sonido;
    private void Start()
    {
        sonido = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        sonido.Play();
    }
}
