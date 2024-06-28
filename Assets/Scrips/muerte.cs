using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class muerte : MonoBehaviour
{
    int sceneIndex;
    [SerializeField] controladorVida vidas;
    // Start is called before the first frame update
    private void Start()
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Eventos.eve.resetCoinsInlvlDied.Invoke();
            Eventos.eve.PasarNivel.Invoke(sceneIndex);
            
        }
        
    }

}
