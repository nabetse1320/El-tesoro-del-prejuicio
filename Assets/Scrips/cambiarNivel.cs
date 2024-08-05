using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class cambiarNivel : MonoBehaviour
{
    [SerializeField] private AnimationClip clipTransition;
    private Animator transitionAnimator;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject hijo;

    private Collider2D col;
    public int SceneSig;
    private float transitionTime;
    private bool bajarVol =false;
    private bool subirVol =false;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<Collider2D>()!=null)
        {
            col= GetComponent<Collider2D>();
        }
        transitionAnimator = hijo.GetComponent<Animator>();
        transitionTime = clipTransition.length;
        hijo.SetActive(true);
        subirVol = true;
        StartCoroutine(FadeInVolume(PlayerPrefs.GetFloat("volumeAll"), transitionTime));
        Invoke("ActivarCanva", transitionTime);

    }
    private void Update()
    {
        
        
        if (videoPlayer!=null)
        {
            if (subirVol && videoPlayer.GetDirectAudioVolume(0)<1)
            {
                videoPlayer.SetDirectAudioVolume(0, videoPlayer.GetDirectAudioVolume(0) + transitionTime * Time.deltaTime);
            }
            else if (videoPlayer.GetDirectAudioVolume(0) == 1)
            {
                subirVol = false;
            }
            if (bajarVol && videoPlayer.GetDirectAudioVolume(0) > 0)
            {
                videoPlayer.SetDirectAudioVolume(0, videoPlayer.GetDirectAudioVolume(0) - transitionTime * Time.deltaTime);
            }
        }
    }

    public void LoadNextScene(int Scene)
    {
        StartCoroutine(SceneLoad(Scene));
    }


    public IEnumerator SceneLoad(int Scene)
    {
        SceneData.sceneToLoad = Scene;
        hijo.SetActive(true);
        StartCoroutine(FadeOutVolume(PlayerPrefs.GetFloat("volumeAll"),transitionTime));
        Time.timeScale = 1;
        transitionAnimator.SetTrigger("StartTransition");
        bajarVol = true;
        yield return new WaitForSeconds(transitionTime);
        //Time.timeScale = 1;
        SceneManager.LoadScene(0);

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Eventos.eve.PausarPlayer.Invoke();
            Eventos.eve.PausarPlayer2.Invoke();
            LoadNextScene(SceneSig);
        }
    }
    IEnumerator FadeInVolume(float endVolume, float duration)
    {
        float currentTime = 0;
        AudioListener.volume = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            AudioListener.volume = Mathf.Lerp(0, endVolume, currentTime / duration);
            yield return null;
        }
        AudioListener.volume = endVolume;
    }
    IEnumerator FadeOutVolume(float startVolume, float duration)
    {
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            AudioListener.volume = Mathf.Lerp(startVolume, 0, currentTime / duration);
            yield return null;
        }
        AudioListener.volume = 0;
    }

    public void ActivarCollider()
    {
        if (col!=null) 
        { 
            col.enabled = true;
        }
    }

    private void ActivarCanva()
    {
        hijo.SetActive(false);
    }
    private void OnEnable()
    {
        Eventos.eve.PasarNivel?.AddListener(LoadNextScene);
    }
    private void OnDisable()
    {
        Eventos.eve.PasarNivel?.RemoveListener(LoadNextScene);
    }
}
