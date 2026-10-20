using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProducirSonido : MonoBehaviour
{
    [SerializeField] private SoundGame secuenceManager;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Color colorInicio;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        colorInicio = GetComponent<SpriteRenderer>().color;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = secuenceManager.sprites[secuenceManager.sounds.IndexOf(audioSource)];
        secuenceManager.RegistrarInstrumento(this);
    }

    public void Sonar()
    {
        if (!secuenceManager.PuzzleIsFinish)
        {
            audioSource.Play();
            int soundIndex = secuenceManager.sounds.IndexOf(audioSource);
            // El aviso de error (rojo) o de exito (verde) lo muestra SoundGame en todos los instrumentos a la vez
            secuenceManager.CheckSound(soundIndex);
        }
    }

    // Cambia el color del instrumento; con duracion > 0 vuelve a su color original al terminar
    public void MostrarColor(Color color, float duracion)
    {
        StopAllCoroutines();
        spriteRenderer.color = color;
        if (duracion > 0f)
        {
            StartCoroutine(RestaurarColor(duracion));
        }
    }

    private IEnumerator RestaurarColor(float duracion)
    {
        yield return new WaitForSeconds(duracion);
        spriteRenderer.color = colorInicio;
    }
}
