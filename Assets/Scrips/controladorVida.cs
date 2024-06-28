using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class controladorVida : MonoBehaviour
{
    [SerializeField] private int vidasRest=3;
    [SerializeField] private GameObject[] vidasImage;
    [SerializeField] private int vida;
    [SerializeField] private float tiempoAnimMuerte=2;
    
    private Slider slider;
    private int sceneIndex;
    private bool morir;
    [Header("Sound Fx")]
    AudioSource audioSource;
    [SerializeField] AudioClip clipMuerte;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        slider = GetComponent<Slider>();
        slider.maxValue = vida;
        slider.value = vida;
        morir = false;

    }
    private void Update()
    {
        
        
        if (slider.value <= 0 && !morir)
        {
            StartCoroutine(animacionMuerteTime());
        }
    }
    IEnumerator animacionMuerteTime()
    {

        Eventos.eve.PausarPlayer.Invoke();
        Eventos.eve.PausarPlayer2.Invoke();
        slider.value = vida;
        vidasRest--;
        for (int i = 0; i < vidasImage.Length; i++)
        {
            if (i == vidasRest)
            {
                vidasImage[i].SetActive(false);
            }
        }
        Eventos.eve.MuertePlayer.Invoke();
        if (vidasRest <= 0 && !morir)
        {
            morir = true;
            StartCoroutine(MuerteDefinitiva());
        }
        else
        {
            yield return new WaitForSeconds(tiempoAnimMuerte);
            Eventos.eve.RevivirPlayer.Invoke();
            slider.value = vida;
        }
    }

    IEnumerator MuerteDefinitiva()
    {
        if (audioSource!=null&&clipMuerte!=null)
        {
            audioSource.clip = clipMuerte;
            audioSource.Play();
        }
        
        Eventos.eve.PausarPlayer.Invoke();
        Eventos.eve.PausarPlayer2.Invoke();
        Eventos.eve.MuertePlayer.Invoke();
        yield return new WaitForSeconds(4);
        Eventos.eve.resetCoinsInlvlDied.Invoke();
        Eventos.eve.PasarNivel.Invoke(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnEnable()
    {
        Eventos.eve.perderVida.AddListener(quitarVida);
    }
    private void OnDisable()
    {
        Eventos.eve.perderVida.RemoveListener(quitarVida);
    }
    private void quitarVida() 
    {
        slider.value--;
    }
}
