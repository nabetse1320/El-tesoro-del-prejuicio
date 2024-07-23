using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePlatform : MonoBehaviour
{
    [SerializeField] private bool _Switch;
    [SerializeField] private GameObject _Platform;
    
    
    public void On()
    {
        _Switch = true;
    }

    public void Off()
    {
        _Switch = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        _Platform.gameObject.SetActive(_Switch);
    }
}
