using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BindingTutorialLeter : MonoBehaviour
{
    [SerializeField] private TextMeshPro texto;
    [SerializeField] private InputActionReference[] actions;
    public string[] textoEntreActions;

    #region Setters & Getters
    public InputActionReference[] Actions { get {return actions;} set {actions=value;} }
    public string[] TextoEntreActions { get {return textoEntreActions;} set {textoEntreActions=value;} }
    #endregion

    void Update()
    {
        // Solo escribe si hay acciones configuradas; si no, deja el texto del cartel (textoIn) intacto
        if (actions!=null && actions.Length > 0)
        {
            String text = "";
            for (int i = 0; i < textoEntreActions.Length; i++)
            {
                text += textoEntreActions[i] + " ";
                if (i % 2 != 1)
                {
                    text += "\"" + actions[i].action.GetBindingDisplayString(0) + "\" ";
                }
            }
            // Evita regenerar el texto de TextMeshPro en cada frame si no cambio
            if (texto.text != text)
            {
                texto.text = text;
            }
        }
        
    }
}

#region
#if UNITY_EDITOR
[CustomEditor (typeof(BindingTutorialLeter))]
public class BindingTutorialLeterEditor : Editor
{
    #region SerializeProperties
    SerializedProperty texto;
    SerializedProperty actions;
    SerializedProperty textoEntreActions;
    #endregion

    private void OnEnable()
    {
        actions = serializedObject.FindProperty("actions");
        textoEntreActions = serializedObject.FindProperty("textoEntreActions");
        texto = serializedObject.FindProperty("texto");
    }

    public override void OnInspectorGUI()
    {
        BindingTutorialLeter btl = (BindingTutorialLeter)target;
        //base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.LabelField("TextObject");
        EditorGUILayout.PropertyField(texto);
        EditorGUILayout.PropertyField(actions);
        if (btl.Actions != null)
        {
            EditorGUILayout.PropertyField(textoEntreActions);
            Array.Resize(ref btl.textoEntreActions, btl.Actions.Length*2);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion
