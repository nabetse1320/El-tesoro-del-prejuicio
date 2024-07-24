using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Cuerda : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float tiempoEspera=1;
    [SerializeField] private bool aparecer;
    [SerializeField] private int id;
    [SerializeField] private GameObject child;
    
    [SerializeField] private bool multiInterruptor = false;
    [SerializeField] private int numeroInterruptores = 2;
    
    [SerializeField] private AudioSource audioSource;

    private int cont;
    private bool act;
    private bool desact;

    #region // getters
    public bool Aparecer{ get { return aparecer; } }
    public bool MultiInterruptor {  get { return multiInterruptor; } }
    #endregion 

    private void Start()
    {
        act = true;
        desact = false;
        if(audioSource != null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (aparecer)
        {
            child.SetActive(false);
            act = false;
            desact = true;
        }
        cont = 0;
    }
    private void Update()
    {
        if (multiInterruptor)
        {
            if (cont == numeroInterruptores)
            {
                if (!act)
                {
                    Activar();
                }
            }
            else
            {
                if (!desact)
                {
                    Desactivar();
                }
            }
        }

    }
    public void Desactivar()
    {
        act = false;
        desact = true;
        StartCoroutine(Espera());
    }
    public void Activar()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        act = true;
        desact = false;
        StartCoroutine(EsperaAct());
    }
    IEnumerator Espera()
    {
        yield return new WaitForSeconds(tiempoEspera);
        if (child != null)
        {
            child.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
    IEnumerator EsperaAct()
    {
        yield return new WaitForSeconds(tiempoEspera);
        if (child!=null)
        {
            child.SetActive(true);
        }
    }
    private void ActivarPlataforma(int idResiver)
    {
        if (idResiver == id)
        {
            if (multiInterruptor)
            {
                if (cont != numeroInterruptores)
                {
                    cont++;
                }
            }
            else
            {
                Activar();
            }
            
        }

    }
    private void DesactivarPlataforma(int idResiver)
    {
        if (idResiver == id)
        {
            if (multiInterruptor)
            {
                cont--;
            }
            else
            {
                Desactivar();
            }
            
        }

    }

    private void OnEnable()
    {
        Eventos.eve.activarCuerda.AddListener(ActivarPlataforma);
        Eventos.eve.DesactivarCuerda.AddListener(DesactivarPlataforma);
    }
    private void OnDisable()
    {
        Eventos.eve.activarCuerda.RemoveListener(ActivarPlataforma);
        Eventos.eve.DesactivarCuerda.RemoveListener(DesactivarPlataforma);
    }
}




//Editor Options

#region
#if UNITY_EDITOR

[CustomEditor (typeof(Cuerda))]
public class CuerdaEditor : Editor
{
    #region SerializedProperties

    SerializedProperty tiempoEspera;
    SerializedProperty aparecer;
    SerializedProperty id;
    SerializedProperty child;
    SerializedProperty multiInterruptor;
    SerializedProperty numeroInterruptores;
    SerializedProperty audioSource;

    bool aparecerGroup,multiInterruptorGroup = false;
    #endregion

    private void OnEnable()
    {
        tiempoEspera = serializedObject.FindProperty("tiempoEspera");
        aparecer = serializedObject.FindProperty("aparecer");
        id = serializedObject.FindProperty("id");
        child = serializedObject.FindProperty("child");
        multiInterruptor = serializedObject.FindProperty("multiInterruptor");
        numeroInterruptores = serializedObject.FindProperty("numeroInterruptores");
        audioSource = serializedObject.FindProperty("audioSource");
    }
    public override void OnInspectorGUI()
    {
        Cuerda cuerda = (Cuerda)target;

        //base.OnInspectorGUI();
        serializedObject.Update();
        //OPCIONES PRINCIPALES -----------------------------------------------------------------------------
        EditorGUILayout.PropertyField(child);
        EditorGUILayout.PropertyField(tiempoEspera);
        EditorGUILayout.PropertyField (audioSource);

        //GRUPO DE OPCIÓNES DE SI EMPIEZA O NO ACTIVO EL OBJETO -------------------------------------------------------------
        EditorGUILayout.Space(10);
        aparecerGroup = EditorGUILayout.BeginFoldoutHeaderGroup(aparecerGroup,"Opciones de si empieza o no activo");
        if (aparecerGroup)
        {
            EditorGUILayout.PropertyField (aparecer);
            if (cuerda.Aparecer)
            {
                EditorGUILayout.LabelField("Id para activarlo:");
                EditorGUILayout.PropertyField(id);
                EditorGUILayout.EndFoldoutHeaderGroup();

                //GRUPO DE OPCIÓNES DE SI SERÁ MULTI-INTERRUPTOR -----------------------------------------------------------------
                EditorGUILayout.Space(10);
                multiInterruptorGroup = EditorGUILayout.BeginFoldoutHeaderGroup(multiInterruptorGroup, "Opciones multi-interruptor");
                if (multiInterruptorGroup)
                {
                    EditorGUILayout.PropertyField(multiInterruptor);
                    if (cuerda.MultiInterruptor)
                    {
                        EditorGUILayout.LabelField("Numero de interruptores necesarios para activar:");
                        EditorGUILayout.PropertyField(numeroInterruptores);

                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion