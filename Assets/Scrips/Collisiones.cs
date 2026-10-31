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
                controladorVida.Dañar();
            }
            // Un cangrejo aturdido por el gancho no hace daño al tocar al jugador
            Cangrejo cangrejo = collision.GetComponent<Cangrejo>();
            if (cangrejo != null && !cangrejo.Aturdido)
            {
                controladorVida.Dañar();
            }
        }
        if (collision.CompareTag("Bala"))
        {
            if (collision.GetComponent<Proyectil>())
            {
                controladorVida.Dañar();
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
