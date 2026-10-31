using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

public class Interruptorgeneral : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private UnityEvent[] activationEvents;
    [SerializeField] private UnityEvent[] deactivationEvents;
    [SerializeField] private bool presionMantenida;
    [SerializeField] private bool esperarTiempo;
    [SerializeField] private float tiempoAEsperar;
    private Animator animator;
    [Header("SoundFX")]
    private AudioSource audioSource;
    [SerializeField] private AudioSource otherAudioSource;
    [SerializeField] private AudioClip audioActivar;
    [SerializeField] private AudioClip audioDesactivar;
    IEnumerator enumerator;
    private bool esperando = false;
    // Lo que esta encima del interruptor (jugador o cajas)
    private readonly HashSet<Collider2D> encima = new HashSet<Collider2D>();
    private bool activado;

    #region Getters y Setters
    public bool EsperarTiempo { get { return esperarTiempo; } }
    public float TiempoAEsperar { get { return tiempoAEsperar; } set { tiempoAEsperar = value; } }
    #endregion

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private static bool Cuenta(Collider2D c)
    {
        return c.CompareTag("Player") || c.CompareTag("ObInteract");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Cuenta(collision)) return;
        bool estabaVacio = encima.Count == 0;
        encima.Add(collision);

        if (esperarTiempo)
        {
            // Temporizado: se activa al pisarlo y se apaga solo pasado el tiempo
            if (!esperando)
            {
                Activar();
                enumerator = EsperandoTiempo();
                StartCoroutine(enumerator);
            }
        }
        else if (estabaVacio || !activado)
        {
            // Solo al pasar de vacio a pisado (antes cada objeto que entraba lo volvia a activar)
            Activar();
        }
    }

    IEnumerator EsperandoTiempo()
    {
        esperando = true;
        yield return new WaitForSeconds(tiempoAEsperar);
        Desactivar();
        esperando = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Se quita aunque ya no tenga la etiqueta (al cambiar de personaje el anterior pasa a Untagged)
        if (encima.Remove(collision))
        {
            ComprobarVacio();
        }
    }

    private void FixedUpdate()
    {
        // Algo encima que se destruye o desactiva no llama a OnTriggerExit2D
        if (encima.Count > 0)
        {
            encima.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
            ComprobarVacio();
        }
    }

    private void ComprobarVacio()
    {
        if (encima.Count > 0) return;
        if (!esperarTiempo && presionMantenida && activado)
        {
            Desactivar();
        }
        else if (!esperarTiempo && !presionMantenida)
        {
            // Sin mantener: queda activado, pero se puede volver a pulsar al bajarse y subirse
            activado = false;
        }
    }

    private void Activar()
    {
        activado = true;
        if (otherAudioSource != null)
        {
            otherAudioSource.Play();
        }
        if (animator != null)
        {
            animator.SetBool("activado", true);
        }
        if (activationEvents != null)
        {
            foreach (var evento in activationEvents)
            {
                evento.Invoke();
            }
        }
        if (audioSource != null)
        {
            audioSource.clip = audioActivar;
            audioSource.Play();
        }
    }

    private void Desactivar()
    {
        activado = false;
        if (animator != null)
        {
            animator.SetBool("activado", false);
        }
        if (audioSource != null)
        {
            audioSource.clip = audioDesactivar;
            audioSource.Play();
        }
        if (deactivationEvents != null)
        {
            foreach (var evento in deactivationEvents)
            {
                evento.Invoke();
            }
        }
    }
}



//Editor Options

#region
#if UNITY_EDITOR
[CustomEditor(typeof(Interruptorgeneral))]
public class InterruptorgeneralEditor : Editor
{
    #region SerializedProperties

    SerializedProperty activationEvents;
    SerializedProperty deactivationEvents;
    SerializedProperty presionMantenida;
    SerializedProperty esperarTiempo;
    SerializedProperty tiempoAEsperar;
    SerializedProperty otherAudioSource;
    SerializedProperty audioActivar;
    SerializedProperty audioDesactivar;

    bool soundGroup = false;
    #endregion

    private void OnEnable()
    {
        activationEvents = serializedObject.FindProperty("activationEvents");
        deactivationEvents = serializedObject.FindProperty("deactivationEvents");
        presionMantenida = serializedObject.FindProperty("presionMantenida");
        esperarTiempo = serializedObject.FindProperty("esperarTiempo");
        tiempoAEsperar = serializedObject.FindProperty("tiempoAEsperar");
        otherAudioSource = serializedObject.FindProperty("otherAudioSource");
        audioActivar = serializedObject.FindProperty("audioActivar");
        audioDesactivar = serializedObject.FindProperty("audioDesactivar");
    }
    public override void OnInspectorGUI()
    {
        Interruptorgeneral interruptor = (Interruptorgeneral)target;

        //base.OnInspectorGUI();
        serializedObject.Update();
        //OPCIONES PRINCIPALES -----------------------------------------------------------------------------
        EditorGUILayout.PropertyField(activationEvents);
        EditorGUILayout.PropertyField(deactivationEvents);

        EditorGUILayout.PropertyField(esperarTiempo);
        if (interruptor.EsperarTiempo)
        {
            EditorGUILayout.PropertyField(tiempoAEsperar);
        }
        else
        {
            EditorGUILayout.PropertyField(presionMantenida);
        }

        //GRUPO DE OPCIÓNES DE SI EMPIEZA O NO ACTIVO EL OBJETO -------------------------------------------------------------
        EditorGUILayout.Space(10);
        soundGroup = EditorGUILayout.BeginFoldoutHeaderGroup(soundGroup, "Opciones de sonido");
        if (soundGroup)
        {
            EditorGUILayout.PropertyField(otherAudioSource);
            EditorGUILayout.PropertyField (audioActivar);
            EditorGUILayout.PropertyField(audioDesactivar);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion
