using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Caja : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private int dañoCaida=10;
    [SerializeField] private float speedToDoDamage=7;
    float cont;

    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip audioRuedo;
    [SerializeField] private AudioClip audioCaida;
    private Vector3 lastPosition;
    private bool sonando;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }
    private void Update()
    {
        lastPosition = transform.position;
        if (rb.velocity.y != 0)
        {
            cont = rb.velocity.y;
        }
        else
        {
            StartCoroutine(ReiniciarCont());
        }
        
    }
    IEnumerator ReiniciarCont()
    {
        yield return new WaitForNextFrameUnit();
        cont = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terreno"))
        {

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.clip = audioCaida;
            audioSource.Play();


        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.GetComponent<Enemigo>())
            {
                if (cont<=-speedToDoDamage)
                {
                    collision.gameObject.GetComponent<Enemigo>().ResivirDaño(dañoCaida);
                }
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Terreno"))
        {
            if (transform.position.x != lastPosition.x || rb.velocity.x != 0)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = audioRuedo;
                    audioSource.Play();
                }
                //audioSource.PlayOneShot(audioRuedo);
            }
        }
    }
}
