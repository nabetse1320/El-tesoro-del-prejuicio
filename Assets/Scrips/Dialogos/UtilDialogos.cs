using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Dialogos sobre el Canvas_dialogos: el panel se abre, el texto se escribe letra por letra
// y se muestra el retrato de quien habla. Lo usan SecuenciaFinal e IntroBoss.
public static class UtilDialogos
{
    [System.Serializable]
    public class Linea
    {
        [Tooltip("Nombre del personaje que habla (Izzy, Liam o Leonardo)")]
        public string personaje;
        [TextArea(2, 4)] public string texto;
    }

    // Crea el canvas desde canvasPrefab, muestra las lineas y lo destruye al terminar.
    // nombreRetrato devuelve el nombre del hijo de "Imagenes" que corresponde a cada personaje.
    // alCrear permite preparar el canvas recien creado (puede ser null).
    public static IEnumerator Mostrar(GameObject canvasPrefab, Linea[] lineas, float tiempoEntreChar,
        System.Func<string, string> nombreRetrato, System.Action<GameObject> alCrear)
    {
        if (canvasPrefab == null || lineas == null || lineas.Length == 0) yield break;

        GameObject canvas = Object.Instantiate(canvasPrefab);
        if (alCrear != null) alCrear(canvas);

        GameObject panel = canvas.transform.Find("PanelDialogos").gameObject;
        TextMeshProUGUI texto = panel.GetComponentInChildren<TextMeshProUGUI>(true);
        Transform imagenes = canvas.transform.Find("Imagenes");

        // El texto usa el rect, margenes, alineacion y fuente del prefab, igual que Dialogos2
        // (los margenes negativos del prefab son el area de texto que usan los dialogos normales)
        texto.text = string.Empty;

        panel.SetActive(true);
        panel.GetComponent<Animator>().SetBool("abrir", true);

        foreach (Linea linea in lineas)
        {
            // Solo queda visible el retrato de quien habla
            string retrato = nombreRetrato != null ? nombreRetrato(linea.personaje) : null;
            if (imagenes != null)
            {
                foreach (Transform hijo in imagenes)
                {
                    hijo.gameObject.SetActive(hijo.name == retrato);
                }
            }

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

        if (imagenes != null)
        {
            foreach (Transform hijo in imagenes)
            {
                hijo.gameObject.SetActive(false);
            }
        }
        panel.GetComponent<Animator>().SetBool("abrir", false);
        yield return new WaitForSeconds(0.5f);
        Object.Destroy(canvas);
    }

    // Misma tecla que Dialogos2 (accion PasarDialogos en Player.prefab): solo Enter
    private static bool PresionoContinuar()
    {
        Keyboard teclado = Keyboard.current;
        return teclado != null && teclado.enterKey.wasPressedThisFrame;
    }
}
