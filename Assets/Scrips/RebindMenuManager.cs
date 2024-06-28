using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    public InputActionReference[] inputActions;
    void Start()
    {
        
    }
    private void OnEnable()
    {
        if(inputActions != null)
        {
            foreach(var action in inputActions)
            {
                action.action.Disable();
            }
        }

        
    }
    private void OnDisable()
    {
        if (inputActions != null)
        {
            foreach (var action in inputActions)
            {
                action.action.Enable();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
