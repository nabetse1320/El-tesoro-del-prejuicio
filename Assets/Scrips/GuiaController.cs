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
    void Start()
    {
        col = GetComponent<Collider2D>();
        globalScale = GetComponentInParent<BoxCollider2D>().transform.lossyScale;
    }
    private void Update()
    {
        if (textMeshPro!=null)
        {
            questions();
        }
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            pergamino.GetComponent<Animator>().SetBool("abrir", true);
            //GetComponentInParent<BoxCollider2D>().transform.transform.localScale = globalScale;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            pergamino.GetComponent<Animator>().SetBool("abrir", false);
        }
    }
    private void questions() {
        string formula = textoIn;
        textMeshPro.text = formula;
    }
}
