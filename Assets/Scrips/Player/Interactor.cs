using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

interface IInteractable
{
    public void Interact();
}

public class Interactor : MonoBehaviour
{
    [SerializeField] private GameObject Interactable;
    [SerializeField] private Collider2D InteractRange;
    // Start is called before the first frame update

    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.TryGetComponent(out IInteractable interactObj))
        {
            Interactable = other.gameObject;
            Debug.Log("Interactuable");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable = null;
        Debug.Log("Exit");
    }

    public void Interact()
    {
        
        
        if (Interactable.TryGetComponent(out IInteractable interactObj)) {
                interactObj.Interact();
                Debug.Log("Interactor");
        }
        
    }
}
