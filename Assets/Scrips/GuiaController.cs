using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GuiaController : MonoBehaviour
{
    Vector2 globalScale;
    public TextMeshPro textMeshPro;
    [SerializeField] private GameObject pergamino;
    private Transform player;
    private bool enElArea=false;
    Collider2D col;
    [TextArea]
    public string textoIn;

    // Colliders de los personajes que estan dentro del trigger. El personaje tiene varios
    // colliders (y al cambiar de personaje cambia cual tiene el tag "Player"), asi que el
    // pergamino solo se cierra cuando ya no queda ninguno dentro.
    private readonly HashSet<Collider2D> collidersDentro = new HashSet<Collider2D>();
    private Animator animatorPergamino;

    void Start()
    {
        col = GetComponent<Collider2D>();
        globalScale = GetComponentInParent<BoxCollider2D>().transform.lossyScale;
        animatorPergamino = pergamino.GetComponent<Animator>();
    }
    private void Update()
    {
        if (textMeshPro!=null)
        {
            questions();
        }

        // Quitar colliders que se desactivaron o destruyeron sin enviar OnTriggerExit2D
        if (collidersDentro.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy) > 0)
        {
            ActualizarPergamino();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (EsPersonaje(collision) && collidersDentro.Add(collision))
        {
            ActualizarPergamino();
            //GetComponentInParent<BoxCollider2D>().transform.transform.localScale = globalScale;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collidersDentro.Remove(collision))
        {
            ActualizarPergamino();
        }
    }

    private bool EsPersonaje(Collider2D collision)
    {
        return collision.CompareTag("Player")
            || collision.GetComponentInParent<PlayerController>() != null
            || collision.GetComponentInParent<PlayerController2>() != null;
    }

    private void ActualizarPergamino()
    {
        bool abierto = collidersDentro.Count > 0;
        if (enElArea == abierto) return;
        enElArea = abierto;
        animatorPergamino.SetBool("abrir", abierto);
    }

    private void questions() {
        // Antes se concatenaba el texto en cada frame y crecia sin limite
        if (!string.IsNullOrEmpty(textoIn) && textMeshPro.text != textoIn)
        {
            textMeshPro.text = textoIn;
        }
    }
}
