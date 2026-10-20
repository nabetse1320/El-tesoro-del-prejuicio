using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Secuencia final del juego: al entrar a la camara del tesoro se muestra el dialogo
// entre Liam e Izzy, luego ambos caminan solos hacia la salida y se pasa a los creditos.
public class SecuenciaFinal : MonoBehaviour
{
    [System.Serializable]
    public class LineaDialogo
    {
        [Tooltip("Liam o Izzy")]
        public string personaje;
        [TextArea(2, 4)] public string texto;
    }

    [Header("Dialogo")]
    [SerializeField] private LineaDialogo[] dialogo;
    [SerializeField] private GameObject canvasDialogosPrefab;
    [SerializeField] private float tiempoEntreChar = 0.03f;

    [Header("Salida")]
    [Tooltip("Punto hacia el que caminan los personajes")]
    [SerializeField] private Transform salida;
    [SerializeField] private float velocidadCaminar = 4f;
    [Tooltip("Distancia entre Liam e Izzy mientras caminan")]
    [SerializeField] private float separacion = 1.6f;
    [SerializeField] private string escenaCreditos = "Creditos";

    private bool iniciado;
    private PlayerSwitch playerSwitch;
    private Rigidbody2D rbIzzy;
    private Rigidbody2D rbLiam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!iniciado && collision.CompareTag("Player"))
        {
            iniciado = true;
            StartCoroutine(Secuencia());
        }
    }

    private IEnumerator Secuencia()
    {
        Eventos.eve.PausarPlayer.Invoke();
        Eventos.eve.PausarPlayer2.Invoke();
        Eventos.eve.CancelSwitches.Invoke();

        playerSwitch = FindObjectOfType<PlayerSwitch>();
        GameObject izzy = playerSwitch.p1;
        GameObject liam = playerSwitch.p2;
        rbIzzy = izzy.GetComponent<Rigidbody2D>();
        rbLiam = liam.GetComponent<Rigidbody2D>();

        // Esperar a que el personaje termine de caer si entró saltando
        float espera = 0f;
        while (espera < 1.5f && Mathf.Abs(rbIzzy.velocity.y) + Mathf.Abs(rbLiam.velocity.y) > 0.05f)
        {
            espera += Time.deltaTime;
            yield return null;
        }
        DetenerAnimacion(izzy);
        DetenerAnimacion(liam);

        yield return StartCoroutine(MostrarDialogo());

        yield return StartCoroutine(CaminarHaciaLaSalida(izzy, liam));
    }

    #region Dialogo

    private IEnumerator MostrarDialogo()
    {
        if (canvasDialogosPrefab == null || dialogo == null || dialogo.Length == 0) yield break;

        GameObject canvas = Instantiate(canvasDialogosPrefab);
        GameObject panel = canvas.transform.Find("PanelDialogos").gameObject;
        TextMeshProUGUI texto = panel.GetComponentInChildren<TextMeshProUGUI>(true);
        GameObject retratoLiam = canvas.transform.Find("Imagenes/Liam").gameObject;
        GameObject retratoIzzy = canvas.transform.Find("Imagenes/Lizzy").gameObject;

        // El texto ocupa el panel dejando espacio a la izquierda para el retrato
        RectTransform rtTexto = texto.rectTransform;
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.pivot = new Vector2(0.5f, 0.5f);
        rtTexto.offsetMin = new Vector2(330f, 40f);
        rtTexto.offsetMax = new Vector2(-80f, -40f);
        texto.enableWordWrapping = true;
        texto.alignment = TextAlignmentOptions.MidlineLeft;
        texto.text = string.Empty;

        panel.SetActive(true);
        panel.GetComponent<Animator>().SetBool("abrir", true);
        yield return new WaitForSeconds(0.3f);

        foreach (LineaDialogo linea in dialogo)
        {
            bool hablaLiam = linea.personaje.Trim().ToLower() == "liam";
            retratoLiam.SetActive(hablaLiam);
            retratoIzzy.SetActive(!hablaLiam);

            // Escribir la linea letra por letra; una pulsacion la completa
            texto.text = string.Empty;
            bool completa = false;
            foreach (char c in linea.texto)
            {
                texto.text += c;
                if (PresionoContinuar())
                {
                    completa = true;
                    break;
                }
                yield return new WaitForSeconds(tiempoEntreChar);
            }
            texto.text = linea.texto;
            if (completa) yield return null;

            // Esperar a que el jugador pase a la siguiente linea
            while (!PresionoContinuar())
            {
                yield return null;
            }
            yield return null;
        }

        retratoLiam.SetActive(false);
        retratoIzzy.SetActive(false);
        panel.GetComponent<Animator>().SetBool("abrir", false);
        yield return new WaitForSeconds(0.5f);
        Destroy(canvas);
    }

    private bool PresionoContinuar()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame || teclado.eKey.wasPressedThisFrame))
            return true;
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;
        Gamepad mando = Gamepad.current;
        if (mando != null && (mando.buttonSouth.wasPressedThisFrame || mando.buttonEast.wasPressedThisFrame))
            return true;
        return false;
    }

    #endregion

    #region Caminata final

    private IEnumerator CaminarHaciaLaSalida(GameObject izzy, GameObject liam)
    {
        // Se quita el control al jugador: los personajes se mueven por script
        playerSwitch.enabled = false;
        izzy.GetComponent<PlayerController>().enabled = false;
        liam.GetComponent<PlayerController2>().enabled = false;
        foreach (GrapplingGun gancho in izzy.GetComponentsInChildren<GrapplingGun>(true))
        {
            if (gancho.isActiveAndEnabled)
            {
                gancho.Canceled();
            }
            gancho.enabled = false;
        }

        // Ambos personajes visibles, uno detras del otro y mirando a la derecha
        GameObject primero = playerSwitch.player1Active ? izzy : liam;
        GameObject segundo = primero == izzy ? liam : izzy;
        segundo.transform.position = primero.transform.position + Vector3.left * separacion;
        izzy.transform.eulerAngles = Vector3.zero;
        liam.transform.eulerAngles = Vector3.zero;
        primero.GetComponent<SpriteRenderer>().sortingOrder = 1;
        segundo.GetComponent<SpriteRenderer>().sortingOrder = 0;
        StartCoroutine(Aparecer(segundo.GetComponent<Renderer>().material, 0.4f));

        foreach (CinemachineVirtualCamera camara in FindObjectsOfType<CinemachineVirtualCamera>())
        {
            camara.Follow = primero.transform;
        }

        AnimarCaminata(izzy);
        AnimarCaminata(liam);

        bool fade = false;
        float tiempo = 0f;
        float destinoX = salida != null ? salida.position.x : primero.transform.position.x + 15f;
        // Siguen caminando durante el fade hasta que cambie la escena
        while (true)
        {
            yield return new WaitForFixedUpdate();
            tiempo += Time.fixedDeltaTime;
            rbIzzy.velocity = new Vector2(velocidadCaminar, rbIzzy.velocity.y);
            rbLiam.velocity = new Vector2(velocidadCaminar, rbLiam.velocity.y);

            if (!fade && (segundo.transform.position.x >= destinoX || tiempo > 20f))
            {
                fade = true;
                int indice = IndiceEscena(escenaCreditos);
                Eventos.eve.PasarNivel.Invoke(indice >= 0 ? indice : 1);
            }
        }
    }

    private IEnumerator Aparecer(Material material, float duracion)
    {
        Color color = material.color;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / duracion);
            material.color = color;
            yield return null;
        }
        color.a = 1f;
        material.color = color;
    }

    private void DetenerAnimacion(GameObject personaje)
    {
        Animator animator = personaje.GetComponent<Animator>();
        SetBoolSiExiste(animator, "Corriendo", false);
        SetBoolSiExiste(animator, "CorriendoEspalda", false);
        personaje.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, personaje.GetComponent<Rigidbody2D>().velocity.y);
    }

    private void AnimarCaminata(GameObject personaje)
    {
        Animator animator = personaje.GetComponent<Animator>();
        SetBoolSiExiste(animator, "Saltando", false);
        SetBoolSiExiste(animator, "Callendo", false);
        SetBoolSiExiste(animator, "CorriendoEspalda", false);
        SetBoolSiExiste(animator, "Corriendo", true);
    }

    private void SetBoolSiExiste(Animator animator, string parametro, bool valor)
    {
        if (animator == null) return;
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == parametro && p.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(parametro, valor);
                return;
            }
        }
    }

    #endregion

    public static int IndiceEscena(string nombre)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string ruta = SceneUtility.GetScenePathByBuildIndex(i);
            if (System.IO.Path.GetFileNameWithoutExtension(ruta) == nombre)
            {
                return i;
            }
        }
        return -1;
    }
}
