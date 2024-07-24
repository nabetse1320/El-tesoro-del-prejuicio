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
    private Collider2D primerObjeto;
    private String tagPO;
    IEnumerator enumerator;
    private bool esperando = false;
    private bool primerTriggerYaEntro;

    #region Getters y Setters
    public bool EsperarTiempo { get { return esperarTiempo; } }
    public float TiempoAEsperar { get { return tiempoAEsperar; } set { tiempoAEsperar = value; } }
    #endregion

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Player") || collision.CompareTag("ObInteract")) && !esperando)
        {
            if (otherAudioSource != null)
            {
                otherAudioSource.Play();
            }
            if (animator != null)
            {
                animator.SetBool("activado", true);
            }
            primerObjeto = collision;
            tagPO = collision.tag;
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
            Debug.Log("Se Activó");
            if (esperarTiempo)
            {
                enumerator = EsperandoTiempo();
                StartCoroutine(enumerator);
            }
        }


    }

    IEnumerator EsperandoTiempo()
    {
        esperando = true;

        yield return new WaitForSeconds(tiempoAEsperar);

        if (animator != null)
        {
            animator.SetBool("activado", false);
        }
        if (audioSource != null)
        {
            audioSource.clip = audioDesactivar;
            audioSource.Play();
        }
        Debug.Log("Se desactivó");
        if (deactivationEvents != null)
        {
            foreach (var evento in deactivationEvents)
            {
                evento.Invoke();

            }
        }
        esperando = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!esperarTiempo)
        {
            if ((collision.CompareTag("Player") || collision.CompareTag("ObInteract")) && presionMantenida)
            {
                if (primerObjeto == collision)
                {
                    primerTriggerYaEntro = false;
                    if (deactivationEvents != null)
                    {
                        foreach (var evento in deactivationEvents)
                        {
                            evento.Invoke();
                        }
                    }
                    if (animator != null)
                    {
                        animator.SetBool("activado", false);
                    }
                    if (audioSource != null)
                    {
                        audioSource.clip = audioDesactivar;
                        audioSource.Play();
                    }

                }
                else if (tagPO == ("Player") && collision.CompareTag("Player"))
                {
                    primerTriggerYaEntro = false;
                    if (deactivationEvents != null)
                    {
                        foreach (var evento in deactivationEvents)
                        {
                            evento.Invoke();
                        }
                    }
                    if (animator != null)
                    {
                        animator.SetBool("activado", false);
                    }
                    if (audioSource != null)
                    {
                        audioSource.clip = audioDesactivar;
                        audioSource.Play();
                    }
                }
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
