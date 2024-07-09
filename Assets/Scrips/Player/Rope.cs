using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
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
    private float currentTime;
    [SerializeField] private float totalDuration;
    [SerializeField] private AnimationCurve myEasingCurve;
    private float t;
    private float easingValue;

    private void Start()
    {
        
        rappeling = false;
        MovementEnd = EndPoint.position;
    }
    private void FixedUpdate()
    {
        if (rappeling)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            Transform PlayerTransform = Player.transform;
            MovementStart = PlayerTransform.position;


            Player.GetComponent<Rigidbody2D>().gravityScale = 0;
            Player.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

            t = Mathf.Clamp01(currentTime / totalDuration);
            easingValue = myEasingCurve.Evaluate(t);
            Vector3 newPosition = Vector3.Lerp(MovementStart, MovementEnd, easingValue);
            Player.transform.position = newPosition;

            currentTime += Time.deltaTime;
            //Player.transform.position = Vector3.Lerp(MovementStart, MovementEnd, Speed * Time.deltaTime);

            if (Math.Abs(Player.transform.position.x - MovementEnd.x) <= threshold)
            {
                EndInteract();
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
        currentTime=0;
        t = 0;
        easingValue = 0;
        rappeling = false;
        Player.GetComponent<Rigidbody2D>().gravityScale = 1;
    }
}
