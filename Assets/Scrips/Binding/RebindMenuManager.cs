using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    public InputActionReference[] inputActions;
    [SerializeField] private RebindSaveLoad rebindSaveLoad;


    void Start()
    {
        //rebindSaveLoad.ChargeRebinds();
    }
    private void OnEnable()
    {
        
        DesactivarInputs();
        


    }
    private void OnDisable()
    {
        rebindSaveLoad.ChargeRebinds();
        ActivarInputs();
    }

    public void ActivarInputs()
    {
        if (inputActions != null)
        {
            foreach (var action in inputActions)
            {
                action.action.Enable();
            }
        }
    }
    public void DesactivarInputs()
    {
        if (inputActions != null)
        {
            foreach (var action in inputActions)
            {
                action.action.Disable();
            }
        }
    }
}
