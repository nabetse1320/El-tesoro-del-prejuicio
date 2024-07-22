using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

interface IInteractable
{
    public void Interact();
    public void EndInteract();
}

public class Interactor : MonoBehaviour
{
    [SerializeField] private GameObject Interactable;
    [SerializeField] private Collider2D InteractRange;
    // Start is called before the first frame update

    #region
    public GameObject InteractableGetter {  get{ return Interactable; } }
    #endregion

    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.TryGetComponent(out IInteractable interactObj))
        {
            Interactable = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable = null;

    }
    public void EndInteract()
    {
        if (Interactable != null)
        {
            if (Interactable.TryGetComponent(out IInteractable interactObj))
            {
                interactObj.EndInteract();
            }
        }
    }

    public void Interact()
    {
        if (Interactable!=null)
        {
            if (Interactable.TryGetComponent(out IInteractable interactObj))
            {
                interactObj.Interact();
            }
        } 
        
    }
}
