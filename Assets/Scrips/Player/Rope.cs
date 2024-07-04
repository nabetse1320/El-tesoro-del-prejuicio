using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject Player;
    private Vector3 MovementStart;
    [SerializeField] private Transform EndPoint;
    private Vector3 velocity = Vector3.zero;
    private Vector3 MovementEnd;
    [SerializeField] private float Speed;

    private void Start()
    {
        MovementEnd = EndPoint.position;
    }

    public void Interact()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Transform PlayerTransform = Player.transform;
        MovementStart = PlayerTransform.position;
        
        Debug.Log("Interactuamos con una tiroleza");


        Player.GetComponent<Rigidbody2D>().gravityScale = 0;
        
        PlayerTransform.position = Vector3.SmoothDamp(MovementStart, MovementEnd,ref velocity, Speed);
        
        Player.GetComponent<Rigidbody2D>().gravityScale = 1;

    }
}
