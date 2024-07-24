using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

public class PlataformaDeslizante : MonoBehaviour
{
    // Start is called before the first frame update
    
    [SerializeField] private bool multiInterruptor=false;
    [SerializeField] private int numeroInterruptores=2;

    
    [SerializeField] private bool iniciaActivado = false;
    [SerializeField] private int idPlataforma;
    
    [SerializeField] private bool idModificable = false;
    [SerializeField] private int maxId=2;
    private Animator animator;
    
    private AudioSource audioSource;
    private int cont;
    private bool act;
    private bool desact;


    #region getters y setters

    public bool MultiInterruptor { get { return multiInterruptor; } }
    public bool IdModificable { get { return idModificable; } }
    public int NumeroInterruptores { get { return numeroInterruptores; } set { numeroInterruptores = value; } }
    public int MaxId {  get { return maxId; } set { maxId = value; } }

    #endregion


    private void Start()
    {
        act = false;
        desact = true;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        cont=0;
        if (iniciaActivado)
        {
            animator.SetBool("deslizar", true);
            act = true;
            desact = false;
        }
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

    public void ActivarPlataforma(int idResiver)
    {
        if (idResiver == idPlataforma)
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
                if (animator.GetBool("deslizar"))
                {
                    Activar();
                }
                else
                {
                    Desactivar();
                }
            }
        }      
    }
    public void Activar()
    {
        animator.SetBool("deslizar", false);
        audioSource.Play();
        if (idModificable)
        {
            if (idPlataforma<maxId)
            {
                idPlataforma++;
            }
        }
        act = true;
        desact = false;
    }
    public void Desactivar()
    {
        animator.SetBool("deslizar", true);
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        if (idModificable)
        {
            if (idPlataforma < maxId)
            {
                idPlataforma++;
            }
        }
        act = false;
        desact = true;
    }
    public void DesactivarPlataforma(int idResiver)
    {
        if (idResiver==idPlataforma)
        {
            if (multiInterruptor)
            {
                cont--;
            }
            else
            {
                if (animator.GetBool("deslizar"))
                {
                    Activar();
                }
                else
                {
                    Desactivar();
                }
            }
        }
    }

    private void OnEnable()
    {
        Eventos.eve.ActivarPlataforma.AddListener(ActivarPlataforma);
        Eventos.eve.DesactivarPlataforma.AddListener(DesactivarPlataforma);
    }
    private void OnDisable()
    {
        Eventos.eve.ActivarPlataforma.RemoveListener(ActivarPlataforma);
        Eventos.eve.DesactivarPlataforma.RemoveListener(DesactivarPlataforma);
    }
}



//Editor Settings

#region
#if UNITY_EDITOR
[CustomEditor(typeof(PlataformaDeslizante))]
public class PlataformaDeslizanteEditor : Editor
{
    #region SerializedProperties

    SerializedProperty multiInterruptor;
    SerializedProperty numeroInterruptores;
    SerializedProperty iniciaActivado;
    SerializedProperty idPlataforma;
    SerializedProperty idModificable;
    SerializedProperty maxId;

    bool multiInterruptorGroup, idModificableGroup = false;
    #endregion

    private void OnEnable()
    {
        multiInterruptor = serializedObject.FindProperty("multiInterruptor");
        numeroInterruptores = serializedObject.FindProperty("numeroInterruptores");
        iniciaActivado = serializedObject.FindProperty("iniciaActivado");
        idPlataforma = serializedObject.FindProperty("idPlataforma");
        idModificable = serializedObject.FindProperty("idModificable");
        maxId = serializedObject.FindProperty("maxId");

        
    }
    public override void OnInspectorGUI()
    {
        PlataformaDeslizante plataforma = (PlataformaDeslizante)target;

        //base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.LabelField("Opciones Generales:");
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(idPlataforma);
        EditorGUILayout.PropertyField(iniciaActivado);
        EditorGUILayout.Space(10);

        multiInterruptorGroup = EditorGUILayout.BeginFoldoutHeaderGroup(multiInterruptorGroup,"Opciones de Multi-interruptor");
        if (multiInterruptorGroup)
        {
            EditorGUILayout.PropertyField(multiInterruptor);
            if (plataforma.MultiInterruptor)
            {
                EditorGUILayout.LabelField("Cantidad de interruptores para poder activar:");
                EditorGUILayout.PropertyField(numeroInterruptores);
                if (plataforma.NumeroInterruptores < 2)
                {
                    EditorGUILayout.HelpBox("La cantidad debe ser mayor a 2",MessageType.Warning);
                    plataforma.NumeroInterruptores = 2;
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        idModificableGroup = EditorGUILayout.BeginFoldoutHeaderGroup(idModificableGroup,"Opciones id incrementable");
        if (idModificableGroup)
        {
            EditorGUILayout.PropertyField (idModificable);
            if (plataforma.IdModificable)
            {
                EditorGUILayout.LabelField("Cantidad maxima a la que puede aumentar el id:");
                EditorGUILayout.PropertyField(maxId);
                if (plataforma.MaxId < 2)
                {
                    EditorGUILayout.HelpBox("La cantidad debe ser mayor a 2", MessageType.Warning);
                    plataforma.MaxId = 2;
                }

            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(10);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion