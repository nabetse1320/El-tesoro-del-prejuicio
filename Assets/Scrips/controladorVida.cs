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
    [Tooltip("Segundos sin recibir daño despues de un golpe o de reaparecer")]
    [SerializeField] private float tiempoInvulnerable = 1f;
    [Tooltip("Cada cuantos segundos alterna el parpadeo durante la invulnerabilidad")]
    [SerializeField] private float intervaloParpadeo = 0.1f;
    [Tooltip("Opacidad de los personajes en la fase tenue del parpadeo")]
    [Range(0f, 1f)]
    [SerializeField] private float alphaParpadeo = 0.3f;

    private Slider slider;
    private int sceneIndex;
    private bool morir;
    private bool muriendo;          // desde que llega a 0 hasta que reaparece
    private float finInvulnerable;  // Time.time hasta el que no recibe daño
    [Header("Sound Fx")]
    AudioSource audioSource;
    [SerializeField] AudioClip clipMuerte;

    private static controladorVida instancia;

    // True mientras el jugador no puede recibir daño (tras un golpe, muriendo o al reaparecer)
    public static bool Invulnerable
    {
        get { return instancia != null && (instancia.muriendo || Time.time < instancia.finInvulnerable); }
    }

    // Unico punto de entrada del daño al jugador: ignora los golpes durante la invulnerabilidad,
    // asi varios golpes casi a la vez (o un golpe que toca varios colliders) quitan solo uno
    public static void Dañar(int cantidad = 1)
    {
        if (instancia == null)
        {
            for (int i = 0; i < cantidad; i++) Eventos.eve.perderVida.Invoke();
            return;
        }
        instancia.RecibirGolpe(cantidad);
    }

    // Caida al vacio: pierde una vida entera y reaparece en el ultimo checkpoint
    public static bool PerderVidaEntera()
    {
        if (instancia == null) return false;
        if (!instancia.muriendo)
        {
            instancia.slider.value = 0;
            instancia.EmpezarMuerte();
        }
        return true;
    }

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
        if (slider.value <= 0)
        {
            EmpezarMuerte();
        }
    }

    private void EmpezarMuerte()
    {
        if (muriendo || morir) return;
        muriendo = true;
        PararParpadeo();
        StartCoroutine(animacionMuerteTime());
    }

    private void RecibirGolpe(int cantidad)
    {
        if (Invulnerable) return;
        finInvulnerable = Time.time + tiempoInvulnerable;
        for (int i = 0; i < cantidad; i++)
        {
            Eventos.eve.perderVida.Invoke();
        }
        EmpezarParpadeo();
    }

    // ---- Parpadeo de los personajes mientras son invulnerables ----
    // Se baja el alpha del color de los SpriteRenderer (el cambio de personaje usa el color del
    // material, asi que no se pisan)
    private readonly List<SpriteRenderer> spritesParpadeo = new List<SpriteRenderer>();
    private readonly List<Color> coloresParpadeo = new List<Color>();
    private Coroutine parpadeo;

    private void EmpezarParpadeo()
    {
        PararParpadeo();
        foreach (PlayerController p in FindObjectsOfType<PlayerController>()) GuardarSprites(p.gameObject);
        foreach (PlayerController2 p in FindObjectsOfType<PlayerController2>()) GuardarSprites(p.gameObject);
        parpadeo = StartCoroutine(Parpadear());
    }

    private void GuardarSprites(GameObject personaje)
    {
        foreach (SpriteRenderer sr in personaje.GetComponentsInChildren<SpriteRenderer>(true))
        {
            spritesParpadeo.Add(sr);
            coloresParpadeo.Add(sr.color);
        }
    }

    private IEnumerator Parpadear()
    {
        bool tenue = false;
        while (Time.time < finInvulnerable && !muriendo)
        {
            tenue = !tenue;
            for (int i = 0; i < spritesParpadeo.Count; i++)
            {
                if (spritesParpadeo[i] == null) continue;
                Color c = coloresParpadeo[i];
                if (tenue) c.a *= alphaParpadeo;
                spritesParpadeo[i].color = c;
            }
            yield return new WaitForSeconds(intervaloParpadeo);
        }
        parpadeo = null;
        PararParpadeo();
    }

    private void PararParpadeo()
    {
        if (parpadeo != null)
        {
            StopCoroutine(parpadeo);
            parpadeo = null;
        }
        for (int i = 0; i < spritesParpadeo.Count; i++)
        {
            if (spritesParpadeo[i] != null) spritesParpadeo[i].color = coloresParpadeo[i];
        }
        spritesParpadeo.Clear();
        coloresParpadeo.Clear();
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
            muriendo = false;
            // Unos instantes sin daño al reaparecer
            finInvulnerable = Time.time + tiempoInvulnerable;
            EmpezarParpadeo();
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
        instancia = this;
        Eventos.eve.perderVida.AddListener(quitarVida);
    }
    private void OnDisable()
    {
        if (instancia == this) instancia = null;
        Eventos.eve.perderVida.RemoveListener(quitarVida);
    }
    private void quitarVida()
    {
        slider.value--;
    }
}
