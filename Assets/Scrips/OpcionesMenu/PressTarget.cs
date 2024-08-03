using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PressTarget : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // Start is called before the first frame update
    [SerializeField] UnityEvent eventoARealizar;
    [SerializeField] UnityEvent eventoDeseleccionar;
    private UnityEngine.UI.Button button;
    [SerializeField] private bool isDefaultButton;
    private GameObject defaultButton;

    private void Awake()
    {
        button = GetComponent<UnityEngine.UI.Button>();
        defaultButton = this.gameObject;

    }
    public void OnEnable()
    {
        if (isDefaultButton)
        {
            EventSystem.current.SetSelectedGameObject(defaultButton);
        }
        else
        {
            eventoDeseleccionar.Invoke();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (eventoARealizar!=null)
        {
            eventoARealizar.Invoke();
        }
        
    }
    public void OnDeselect(BaseEventData eventData) 
    {
        if (eventoDeseleccionar!=null)
        {
            eventoDeseleccionar.Invoke();
            
        }
    }

    public void ActivarOnclick()
    {
        button.onClick.Invoke();
    }
}



//#region
//[CustomEditor(typeof (PressTarget))]
//public class PressTargetEditor: Editor
//{
//    #region
//    SerializedProperty defaultButton;
//    SerializedProperty 
//    #endregion
//}
//#endregion
