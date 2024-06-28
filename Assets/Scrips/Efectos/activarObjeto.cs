using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activarObjeto : MonoBehaviour
{
    [SerializeField] private GameObject[] objetos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (var objeto in objetos)
            {
                objeto.SetActive(false);
            }
        }
    }
}
