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
            // Pierde una vida entera y reaparece en el ultimo checkpoint (al quedarse sin vidas
            // el controlador de vida reinicia el nivel). Sin controlador de vida en la escena se
            // reinicia el nivel como antes
            if (controladorVida.PerderVidaEntera()) return;
            Eventos.eve.resetCoinsInlvlDied.Invoke();
            ResetLvl();
        }
        
    }
    public void ResetLvl()
    {
        Eventos.eve.PasarNivel.Invoke(sceneIndex);
    }

}
