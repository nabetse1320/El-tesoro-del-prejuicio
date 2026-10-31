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

    // Personaje que esta llamando a Interact() (la tirolina y las cuerdas lo usan para saber a quien mover)
    public static GameObject JugadorInteractuando { get; private set; }

    // Lo que se activo con Interact(): al soltar la tecla se termina aunque el personaje ya haya
    // salido de su trigger (en la tirolina o la cuerda se mueve con el personaje agarrado)
    private IInteractable enUso;
    
    private void OnTriggerStay2D(Collider2D other)
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
        if (enUso != null)
        {
            IInteractable anterior = enUso;
            enUso = null;
            if ((anterior as UnityEngine.Object) != null)
            {
                anterior.EndInteract();
            }
            return;
        }
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
                enUso = interactObj;
                JugadorInteractuando = gameObject;
                interactObj.Interact();
                JugadorInteractuando = null;
            }
        }
        
    }
}
