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
    private bool rappeling;

    private void Start()
    {
        rappeling = false;
        MovementEnd = EndPoint.position;
    }
    private void Update()
    {
        if (rappeling)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            Transform PlayerTransform = Player.transform;
            MovementStart = PlayerTransform.position;

            Debug.Log("Interactuamos con una tiroleza");


            Player.GetComponent<Rigidbody2D>().gravityScale = 0;
            Player.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            Player.transform.position = Vector3.Lerp(MovementStart, MovementEnd, Speed * Time.deltaTime);
        }
    }

    public void Interact()
    {
        
        
        rappeling = true;
        
        //Player.GetComponent<Rigidbody2D>().gravityScale = 1;

    }
}
