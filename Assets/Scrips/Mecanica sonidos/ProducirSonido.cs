using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProducirSonido : MonoBehaviour
{
    [SerializeField] private SoundGame secuenceManager;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Sonar()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        int soundIndex = secuenceManager.sounds.IndexOf(audioSource);
        secuenceManager.CheckSound(soundIndex);
    }
}
