using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteract : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interactuamos con un objeto");
    }
}
