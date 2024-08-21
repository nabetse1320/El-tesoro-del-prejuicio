using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    private bool _hasDropped;
    [SerializeField] private GameObject drop;
    [SerializeField] private Transform dropPoint;
    private GameObject currentInstantiate;
    
    void Start()
    {
        _hasDropped = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_hasDropped)
        {
            currentInstantiate = Instantiate(drop, dropPoint.position, Quaternion.identity);
            _hasDropped = true;
        }
    }

    public void ResetDrop()
    {
        Destroy(currentInstantiate);
        _hasDropped = false;
    }
}
