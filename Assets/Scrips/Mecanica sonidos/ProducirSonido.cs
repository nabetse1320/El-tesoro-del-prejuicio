using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProducirSonido : MonoBehaviour
{
    [SerializeField] private SoundGame secuenceManager;
    private AudioSource audioSource;
    private Color colorInicio;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        colorInicio = GetComponent<SpriteRenderer>().color;
    }

    public void Sonar()
    {
        if (!secuenceManager.PuzzleIsFinish)
        {
            //if (!audioSource.isPlaying)
            //{
            //    audioSource.Play();
            //}

            audioSource.Play();
            int soundIndex = secuenceManager.sounds.IndexOf(audioSource);
            if (soundIndex != secuenceManager.sequence[secuenceManager.currentSound])
            {
                this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            secuenceManager.CheckSound(soundIndex);
            Invoke("ChangeColorWhite",0.5f);
        } 
    }
    private void ChangeColorWhite()
    {
        this.gameObject.GetComponent<SpriteRenderer>().color = colorInicio;
    }
}
