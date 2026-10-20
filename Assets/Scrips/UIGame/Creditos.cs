using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Muestra los creditos uno por uno (nombre + funcion) con fade in / fade out
// y al terminar regresa al menu principal.
public class Creditos : MonoBehaviour
{
    [System.Serializable]
    public class Credito
    {
        public string nombre;
        public string funcion;
    }

    [SerializeField] private Credito[] creditos;
    [SerializeField] private TMP_FontAsset fuente;
    [SerializeField] private Color colorNombre = new Color(0.93f, 0.8f, 0.45f, 1f);
    [SerializeField] private Color colorFuncion = Color.white;

    [Header("Tiempos")]
    [SerializeField] private float esperaInicial = 1.5f;
    [SerializeField] private float duracionFadeIn = 1f;
    [SerializeField] private float tiempoVisible = 2.5f;
    [SerializeField] private float duracionFadeOut = 1f;
    [SerializeField] private float pausaEntreNombres = 0.5f;

    [Header("Salida")]
    [SerializeField] private string escenaMenu = "Menú";
    [SerializeField] private AudioSource musica;

    private CanvasGroup grupo;
    private TextMeshProUGUI textoNombre;
    private TextMeshProUGUI textoFuncion;

    private void Start()
    {
        Time.timeScale = 1;
        // La transicion de la escena anterior deja el volumen general en 0
        AudioListener.volume = PlayerPrefs.HasKey("volumeAll") ? PlayerPrefs.GetFloat("volumeAll") : 1f;
        CrearInterfaz();
        StartCoroutine(MostrarCreditos());
    }

    private void CrearInterfaz()
    {
        GameObject canvasGO = new GameObject("CanvasCreditos", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        grupo = canvasGO.GetComponent<CanvasGroup>();
        grupo.alpha = 0f;

        textoNombre = CrearTexto(canvasGO.transform, "Nombre", new Vector2(0f, 40f), 80f, colorNombre, FontStyles.Bold);
        textoFuncion = CrearTexto(canvasGO.transform, "Funcion", new Vector2(0f, -60f), 48f, colorFuncion, FontStyles.Normal);
    }

    private TextMeshProUGUI CrearTexto(Transform padre, string nombre, Vector2 posicion, float tamano, Color color, FontStyles estilo)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1600f, 120f);
        rt.anchoredPosition = posicion;
        TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
        if (fuente != null) texto.font = fuente;
        texto.fontSize = tamano;
        texto.color = color;
        texto.fontStyle = estilo;
        texto.alignment = TextAlignmentOptions.Center;
        texto.enableWordWrapping = true;
        return texto;
    }

    private IEnumerator MostrarCreditos()
    {
        yield return new WaitForSeconds(esperaInicial);

        foreach (Credito credito in creditos)
        {
            textoNombre.text = credito.nombre;
            textoFuncion.text = credito.funcion;
            yield return StartCoroutine(Fade(0f, 1f, duracionFadeIn));
            yield return new WaitForSeconds(tiempoVisible);
            yield return StartCoroutine(Fade(1f, 0f, duracionFadeOut));
            yield return new WaitForSeconds(pausaEntreNombres);
        }

        // Bajar la musica antes de volver al menu
        if (musica != null)
        {
            float volumenInicial = musica.volume;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                musica.volume = Mathf.Lerp(volumenInicial, 0f, t);
                yield return null;
            }
        }

        int indiceMenu = SecuenciaFinal.IndiceEscena(escenaMenu);
        SceneData.sceneToLoad = indiceMenu >= 0 ? indiceMenu : 1;
        SceneManager.LoadScene(0);
    }

    private IEnumerator Fade(float desde, float hasta, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            grupo.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        grupo.alpha = hasta;
    }
}
