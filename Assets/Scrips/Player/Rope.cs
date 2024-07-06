using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject Player;
    private Vector3 MovementStart;
    [SerializeField] private Transform EndPoint;
    [SerializeField] private float threshold;
    private Vector3 velocity = Vector3.zero;
    private Vector3 MovementEnd;
    [SerializeField] private float Speed;
    [SerializeField] private bool rappeling;

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


            Player.GetComponent<Rigidbody2D>().gravityScale = 0;
            Player.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            Player.transform.position = Vector3.Lerp(MovementStart, MovementEnd, Speed * Time.deltaTime);

            if (Math.Abs(Player.transform.position.x - MovementEnd.x) <= threshold)
            {
                rappeling = false;
                Player.GetComponent<Rigidbody2D>().gravityScale = 1;
            }

        }
    }

    public void Interact()
    {

        
        rappeling = true;
        
        //Player.GetComponent<Rigidbody2D>().gravityScale = 1;

    }
    public void EndInteract()
    {
        rappeling = false;
        Player.GetComponent<Rigidbody2D>().gravityScale = 1;
    }
}
