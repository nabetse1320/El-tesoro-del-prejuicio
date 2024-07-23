using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Liana : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject player;
    private Vector3 MovementStart;
    [SerializeField] private Transform EndPoint;
    [SerializeField] private float threshold;
    private Vector3 velocity = Vector3.zero;
    private Vector3 MovementEnd;
    private bool rappeling;
    private float currentTime;
    [SerializeField] private float totalDuration;
    [SerializeField] private AnimationCurve myEasingCurve;
    private float t;
    private float easingValue;

    private void Start()
    {

        rappeling = false;

    }
    private void FixedUpdate()
    {
        MovementEnd = EndPoint.position;
        player = GameObject.FindGameObjectWithTag("Player");
        if (rappeling)
        {
            Transform PlayerTransform = player.transform;
            MovementStart = PlayerTransform.position;
            Eventos.eve.CancelSwitches.Invoke();
            player.GetComponent<Rigidbody2D>().gravityScale = 0;
            player.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

            t = Mathf.Clamp01(currentTime / totalDuration);
            easingValue = myEasingCurve.Evaluate(t);
            Vector3 newPosition = Vector3.Lerp(MovementStart, MovementEnd, easingValue);
            player.transform.position = newPosition;

            currentTime += Time.deltaTime;

            if (player.GetComponent<Interactor>().InteractableGetter!=this.gameObject)
            {
                EndInteract();
            }
            if (Math.Abs(player.transform.position.y - MovementEnd.y) <= threshold)
            {
                EndInteract();
            }

        }
    }

    public void Interact()
    {
        rappeling = true;
    }
    public void EndInteract()
    {
        Eventos.eve.ActivateSwitches.Invoke();
        currentTime = 0;
        t = 0;
        easingValue = 0;
        rappeling = false;
        player.GetComponent<Rigidbody2D>().gravityScale = 1;
    }
}
