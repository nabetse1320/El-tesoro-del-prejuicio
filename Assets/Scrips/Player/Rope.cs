using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class Rope : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject Player;
    private AudioSource audioSource;
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
        audioSource = GetComponent<AudioSource>();
        rappeling = false;
        
    }
    private void FixedUpdate()
    {
        
        MovementEnd = EndPoint.position;
        Player = GameObject.FindGameObjectWithTag("Player");
        if (rappeling&&Player.layer== 7)
        {
            Transform PlayerTransform = Player.transform;
            MovementStart = PlayerTransform.position;
            Eventos.eve.CancelSwitches.Invoke();
            Eventos.eve.PausarPlayer.Invoke();
            Eventos.eve.PausarPlayer2.Invoke();
            Player.GetComponent<Rigidbody2D>().gravityScale = 0;
            Player.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;

            t = Mathf.Clamp01(currentTime / totalDuration);
            easingValue = myEasingCurve.Evaluate(t);
            Vector3 newPosition = Vector3.Lerp(MovementStart, MovementEnd, easingValue);
            Player.transform.position = newPosition;

            currentTime += Time.deltaTime;


            if (Math.Abs(Player.transform.position.x - MovementEnd.x) <= threshold)
            {
                EndInteract();
            }

        }
        if (Player.GetComponent<PlayerController>())
        {
            if (Player.GetComponent<PlayerController>().muerto)
            {
                EndInteract();
            }
        }
    }

    public void Interact()
    {
        // Solo Izzy puede usar la tirolina. Antes, si Liam la activaba se reproducia el sonido
        // y quedaba "armada", y al cambiar a Izzy se movia desde donde estuviera.
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (rappeling || jugador == null || jugador.GetComponent<PlayerController>() == null)
        {
            return;
        }
        Player = jugador;
        if (audioSource != null)
        {
            audioSource.Play();
        }
        rappeling = true;
    }
    public void EndInteract()
    {
        // Si no se estaba usando la tirolina no hay nada que terminar (evita despausar o
        // cambiar la gravedad del jugador al soltar la tecla de interactuar en otro momento)
        if (!rappeling)
        {
            return;
        }
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        Eventos.eve.ActivateSwitches.Invoke();
        Eventos.eve.DespausarPlayer.Invoke();
        Eventos.eve.DespausarPlayer2.Invoke();
        currentTime =0;
        t = 0;
        easingValue = 0;
        rappeling = false;
        Player.GetComponent<Rigidbody2D>().gravityScale = 1;
    }
}
