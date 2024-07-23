using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collisiones : MonoBehaviour
{
    [Header("Si collisiona con 'Detener' se ejecutaran estos eventos")]
    [SerializeField] UnityEvent[] eventosADetener;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) 
        {
            if (collision.GetComponent<Fantasma>())
            {
                Eventos.eve.perderVida.Invoke();
            }
            if (collision.GetComponent<Cangrejo>())
            {
                Eventos.eve.perderVida.Invoke();
            }
        }
        if (collision.CompareTag("Bala"))
        {
            if (collision.GetComponent<Proyectil>())
            {
                Eventos.eve.perderVida.Invoke();
                collision.GetComponent<Proyectil>().Destruir();
            }
        }
        if (collision.CompareTag("Detener")&&this.gameObject.CompareTag("Player"))
        {
            if (eventosADetener!=null)
            {
                foreach (var e in eventosADetener) { e.Invoke();}
            }
            Eventos.eve.PausarPlayer.Invoke();
            Eventos.eve.PausarPlayer2.Invoke();
        }
    }


}
