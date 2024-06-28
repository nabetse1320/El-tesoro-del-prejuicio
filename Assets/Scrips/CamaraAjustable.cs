using UnityEngine;
using Cinemachine;

public class ZonaTrigger : MonoBehaviour
{
    public CinemachineVirtualCamera camaraVirtual;
    public CinemachineVirtualCamera[] otrasCamaras;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Activar la cámara virtual de esta zona
            camaraVirtual.Priority = 1;

            // Desactivar todas las demás cámaras
            foreach (var camara in otrasCamaras)
            {
                camara.Priority = 0;
            }
        }
    }
}
