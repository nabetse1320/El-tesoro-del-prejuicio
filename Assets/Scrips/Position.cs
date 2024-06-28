using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Position : MonoBehaviour
{
    public GameObject Objeto;
    private void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().sortingLayerName = "otros";
        this.gameObject.GetComponent<MeshRenderer>().sortingOrder = 1;
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;   
        position = Objeto.transform.position;
        transform.position = position;
  
    }
}
